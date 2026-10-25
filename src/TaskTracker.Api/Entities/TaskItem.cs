namespace TaskTracker.Api.Entities;

public enum TaskStatus { Todo, InProgress, Done }
public enum TaskPriority { Low, Medium, High, Critical }

public sealed class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public required string Title { get; set; }
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public Guid? AssigneeId { get; set; }
    public User? Assignee { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<Comment> Comments { get; set; } = [];
}
