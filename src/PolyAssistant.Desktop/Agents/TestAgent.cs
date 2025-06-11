using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using PolyAssistant.Core.Agents;
using PolyAssistant.Core.Agents.Interfaces;

namespace PolyAssistant.Desktop.Agents;

public sealed class TestAgent(ILogger<TestAgent> logger, IServiceProvider serviceProvider) : DesktopAgent<TestAgent>(logger, serviceProvider)
{
    public override string Name => "Test Agent";

    protected override IAgentVoice DefaultVoice => new ChatterboxAgentVoice("margot_robbie_ex.mp3");

    protected override IAgentVoice DefaultVoiceForUnrecognizedInvocation => new ChatterboxAgentVoice("margot_robbie_ex.mp3")
    {
        Exaggeration = 0.35,
        Pace = 0.8f
    };

    [KernelFunction]
    [Description("Runs the calculator app")]
    public void RunCalculator()
    {
        Process.Start("calc.exe");

        _ = SpeakAsync(WordCategory.Ok);
    }

    // TODO: figure out how to query an LLM about a JSON node
    //
    //

    /*

    [KernelFunction]
    [Description("Finds and clicks an element on screen.")]
    public async Task Click(string query)
    {
        var tree = Automation.GetVisualTreeFromFocusedElement();
        var json = JsonSerializer.Serialize(tree);

        var arguments = new KernelArguments
        {
            { "contextData", json },
            { "userQuery", $"Find an element by Name or AutomationId that represents '{query}'" }
        };

        var prompt =
            "Based on the following JSON data:\n" +
            "{{$contextData}}\n" +
            "and the user's query: '{{$userQuery}}', find the return the JSON node best fits the user's query.\n" +
            "Only respond with the matching JSON from the context. Nothing else. Do not include any markdown syntax.";

        var result = await Kernel.InvokePromptAsync(prompt, arguments);
        var str = result.GetValue<string>();

        if (string.IsNullOrWhiteSpace(str))
        {
            return;
        }

        var node = JsonSerializer.Deserialize<VisualTreeNode>(str);
    }

    */
}