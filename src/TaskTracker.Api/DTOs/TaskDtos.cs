using TaskTracker.Api.Entities;
using TaskStatus = TaskTracker.Api.Entities.TaskStatus;

namespace TaskTracker.Api.DTOs;

public sealed record CreateTaskRequest(string Title, string? Description, TaskPriority Priority, DateTime? DueDate, Guid? AssigneeId);
public sealed record UpdateTaskRequest(string Title, string? Description, TaskStatus Status, TaskPriority Priority, DateTime? DueDate, Guid? AssigneeId);
public sealed record TaskResponse(Guid Id, Guid ProjectId, string Title, string? Description, TaskStatus Status, TaskPriority Priority, DateTime? DueDate, Guid? AssigneeId, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);
public sealed class TaskQuery
{
    public TaskStatus? Status { get; init; }
    public Guid? AssigneeId { get; init; }
    public string SortBy { get; init; } = "createdAt";
    public bool Descending { get; init; } = true;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
public sealed record TaskStatusCount(TaskStatus Status, int Count);
