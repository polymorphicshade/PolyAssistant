using Microsoft.EntityFrameworkCore;

namespace PolyAssistant.Core.Data.Chat;

public sealed class ChatHistoryDbContext : DbContext
{
    public ChatHistoryDbContext(DbContextOptions<ChatHistoryDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<ChatConversationEntity> Conversations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ChatMessageEntity

        modelBuilder
            .Entity<ChatMessageEntity>()
            .HasKey(x => x.Id);

        modelBuilder
            .Entity<ChatMessageEntity>()
            .Property(x => x.TimeUtc);

        modelBuilder
            .Entity<ChatMessageEntity>()
            .Property(x => x.Role);

        modelBuilder
            .Entity<ChatMessageEntity>()
            .Property(x => x.Content);

        // ChatConversationEntity

        modelBuilder
            .Entity<ChatConversationEntity>()
            .HasKey(x => x.Id);

        modelBuilder
            .Entity<ChatConversationEntity>()
            .Property(x => x.TimeUtc);

        modelBuilder
            .Entity<ChatConversationEntity>()
            .HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId);
    }
}