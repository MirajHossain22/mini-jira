namespace TaskTracker.Api.Entities;

public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<Project> Projects { get; set; } = [];
    public ICollection<TaskItem> AssignedTasks { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
}
