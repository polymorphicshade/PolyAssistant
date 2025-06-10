using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using PolyAssistant.Core.Configuration;
using PolyAssistant.Core.Data.Chat;
using PolyAssistant.Core.Models.Chat;
using PolyAssistant.Core.Services.Interfaces;

namespace PolyAssistant.Core.Services;

public sealed class OllamaService : IOllamaService
{
    private readonly ILogger<OllamaService> _logger;
    private readonly ChatHistoryDbContext _chatHistoryDbContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<OllamaConfiguration> _configuration;
    private readonly Kernel _kernel;

    public OllamaService(ILogger<OllamaService> logger, ChatHistoryDbContext chatHistoryDbContext, IHttpClientFactory httpClientFactory, IOptions<OllamaConfiguration> configuration)
    {
        _logger = logger;
        _chatHistoryDbContext = chatHistoryDbContext;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;

        var url = $"{configuration.Value.Url}/v1";
        var defaultModel = configuration.Value.DefaultModel;
        var defaultVisionModel = configuration.Value.DefaultVisionModel;

        var httpClient = httpClientFactory.CreateClient();

        //
        // TODO: allow Ollama keep_alive customization somehow
        //

        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(defaultModel, apiKey: null, endpoint: new Uri(url), httpClient: httpClient, serviceId: "chat");
        builder.AddOpenAIChatCompletion(defaultVisionModel, apiKey: null, endpoint: new Uri(url), httpClient: httpClient, serviceId: "vision");

        _logger.LogInformation("Using URL: {str}", url);
        _logger.LogInformation("Using default model: {str}", defaultModel);
        _logger.LogInformation("Using default model (vision): {str}", defaultVisionModel);
        _logger.LogInformation("Total conversations in cache: {count}", _chatHistoryDbContext.Conversations.Count());

        _kernel = builder.Build();
    }

    public async Task ClearConversationsAsync(IEnumerable<Guid>? conversationIds = null, CancellationToken cancellationToken = default)
    {
        if (conversationIds == null)
        {
            await _chatHistoryDbContext.Conversations.ExecuteDeleteAsync(cancellationToken);

            return;
        }

        foreach (var id in conversationIds)
        {
            var conversation =
                _chatHistoryDbContext
                    .Conversations
                    .FirstOrDefault(x => x.Id == id);

            if (conversation == null)
            {
                return;
            }

            _chatHistoryDbContext.Remove(conversation);
        }

        await _chatHistoryDbContext.SaveChangesAsync(cancellationToken);
    }

    public IEnumerable<ChatConversationModel> GetConversationsAsync(int conversationTopCount = 5, int messagesTopCount = 10, DateTime? conversationFrom = null, DateTime? conversationTo = null, DateTime? messageFrom = null, DateTime? messageTo = null, CancellationToken cancellationToken = default)
    {
        conversationFrom ??= DateTime.MinValue;
        conversationTo ??= DateTime.MaxValue;

        messageFrom ??= DateTime.MinValue;
        messageTo ??= DateTime.MaxValue;

        return
            _chatHistoryDbContext
                .Conversations
                .Include(x =>
                    x.Messages
                        .Where(y => messageFrom.Value <= y.TimeUtc && y.TimeUtc < messageTo)
                        .Take(messagesTopCount))
                .Where(x => conversationFrom <= x.TimeUtc && x.TimeUtc < conversationTo)
                .Take(conversationTopCount)
                .Select(x => x.ToModel());
    }

    public IEnumerable<ChatConversationModel> GetConversationsAsync(Guid[] ids, CancellationToken cancellationToken = default)
    {
        var lookup = ids.ToHashSet();

        return
            _chatHistoryDbContext
                .Conversations
                .Include(x => x.Messages)
                .Where(x => lookup.Contains(x.Id))
                .Select(x => x.ToModel());
    }

    public async Task<ChatResponseModel> ChatAsync(string message, string? systemMessage = null, string? model = null, string? imageContext = null, OpenAIPromptExecutionSettings? settings = null, Guid? conversationId = null, bool isAtomic = false, CancellationToken cancellationToken = default)
    {
        systemMessage ??= _configuration.Value.DefaultSystemMessage;

        if (imageContext != null)
        {
            model ??= _configuration.Value.DefaultVisionModel;
        }
        else
        {
            model ??= _configuration.Value.DefaultModel;
        }

        settings ??= new OpenAIPromptExecutionSettings();

        settings.ModelId = model;

        _logger.LogInformation("Sent: {message}", message);

        var content = new ChatMessageContentItemCollection
        {
            new TextContent(message)
        };

        if (imageContext != null)
        {
            content.Add(new ImageContent(imageContext));
        }

        var chatCompletionServiceId = imageContext == null 
            ? "chat" 
            : "vision";

        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>(chatCompletionServiceId);

        var response = await DoSendAsync(content, chatCompletionService, systemMessage, model, settings, conversationId, !isAtomic, cancellationToken);

        _logger.LogInformation("Received: {response}", response.Message);

        return response;
    }

    public async Task<ChatConversationModel?> ResetConversationAsync(Guid conversationId, long? messageId = null, CancellationToken cancellationToken = default)
    {
        var conversation = await _chatHistoryDbContext
            .Conversations
            .Include(chatConversationEntity => chatConversationEntity.Messages)
            .FirstOrDefaultAsync(x => x.Id == conversationId, cancellationToken);

        // ReSharper disable once UseNullPropagation
        if (conversation == null)
        {
            return null;
        }

        if (messageId == null)
        {
            conversation.Messages.Clear();

            await _chatHistoryDbContext.SaveChangesAsync(cancellationToken);

            return conversation.ToModel();
        }

        var message = conversation.Messages.FirstOrDefault(x => x.Id == messageId);

        if (message == null)
        {
            return null;
        }

        foreach (var item in conversation.Messages.OrderBy(x => x.TimeUtc).ToArray())
        {
            if (item.TimeUtc > message.TimeUtc)
            {
                conversation.Messages.Remove(item);
            }
        }

        await _chatHistoryDbContext.SaveChangesAsync(cancellationToken);

        return conversation.ToModel();
    }

    public async Task CreateModelAsync(string path, string modelfile, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task DownloadModelAsync(string url, string destinationPath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    // helpers

    private async Task PullModelAsync(string model, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Pulling model: {model}", model);

        var httpClient = _httpClientFactory.CreateClient();

        httpClient.Timeout = TimeSpan.FromSeconds(1800);

        var response = await httpClient.PostAsJsonAsync($"{_configuration.Value.Url}/api/pull", new
        {
            name = model,
            stream = false
        }, cancellationToken);

        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Model pulled successfully");
    }

    // TODO: change to store ChatMessageContent JSON instead of just the message (for things like images)
    private async Task<ChatResponseModel> DoSendAsync(ChatMessageContentItemCollection content, IChatCompletionService chatCompletionService, string systemMessage, string model, OpenAIPromptExecutionSettings settings, Guid? conversationId, bool saveNewConversation, CancellationToken cancellationToken = default)
    {
        ChatConversationEntity? conversation = null;

        if (conversationId == null)
        {
            conversationId ??= Guid.NewGuid();
        }
        else
        {
            conversation =
                _chatHistoryDbContext
                    .Conversations
                    .Include(x => x.Messages)
                    .FirstOrDefault(x => x.Id == conversationId);
        }

        if (conversation == null)
        {
            conversation = new ChatConversationEntity
            {
                Id = conversationId.Value
            };

            _chatHistoryDbContext.Conversations.Add(conversation);
        }

        if (conversation.Messages.Count == 0)
        {
            conversation.Messages.Add(new ChatMessageEntity
            {
                Role = AuthorRole.System.Label,
                Content = systemMessage,
                ConversationId = conversation.Id,
                Conversation = conversation
            });
        }

        conversation.Messages.Add(new ChatMessageEntity
        {
            Role = AuthorRole.User.Label,
            Content = JsonSerializer.Serialize(content),
            ConversationId = conversation.Id,
            Conversation = conversation
        });

        var chatHistory = new ChatHistory();

        foreach (var item in conversation.Messages.OrderBy(x => x.TimeUtc))
        {
            switch (item.Role)
            {
                case var role when role == AuthorRole.Assistant.Label:
                    chatHistory.AddAssistantMessage(item.Content);
                    break;
                case var role when role == AuthorRole.User.Label:
                    var userMessage = JsonSerializer.Deserialize<ChatMessageContentItemCollection>(item.Content);
                    chatHistory.AddUserMessage(userMessage);
                    break;
                case var role when role == AuthorRole.System.Label:
                    chatHistory.AddSystemMessage(item.Content);
                    break;
            }
        }

        var response = await GetChatMessageContentAsync(chatHistory, chatCompletionService, settings, model, true, cancellationToken);

        var responseMessage = response.Content ?? string.Empty;

        conversation.Messages.Add(new ChatMessageEntity
        {
            Role = AuthorRole.Assistant.Label,
            Content = responseMessage,
            ConversationId = conversation.Id,
            Conversation = conversation
        });

        if (saveNewConversation)
        {
            await _chatHistoryDbContext.SaveChangesAsync(cancellationToken);
        }

        return new ChatResponseModel
        {
            ConversationId = saveNewConversation ? conversationId : null,
            Message = responseMessage
        };
    }

    private async Task<ChatMessageContent> GetChatMessageContentAsync(ChatHistory chatHistory, IChatCompletionService chatCompletionService, OpenAIPromptExecutionSettings settings, string model, bool handleMissingModel, CancellationToken cancellationToken = default)
    {
        try
        {
            return await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, _kernel, cancellationToken);
        }
        catch (Exception ex)
        {
            if (ex.RepresentsMissingModel(model) && handleMissingModel)
            {
                _logger.LogWarning("Model is missing: {model}", model);

                // pull the model
                await PullModelAsync(model, cancellationToken);

                // try again (only once)
                return await GetChatMessageContentAsync(chatHistory, chatCompletionService, settings, model, false, cancellationToken);
            }

            throw;
        }
    }
}