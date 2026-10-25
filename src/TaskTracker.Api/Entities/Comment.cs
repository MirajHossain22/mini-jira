namespace TaskTracker.Api.Entities;

public sealed class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;
    public required string Body { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
