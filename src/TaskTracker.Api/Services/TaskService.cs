using TaskTracker.Api.Common;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Entities;
using TaskTracker.Api.Repositories;
using TaskStatus = TaskTracker.Api.Entities.TaskStatus;

namespace TaskTracker.Api.Services;

public sealed class TaskService(ITaskRepository tasks, IProjectRepository projects, IUserRepository users, ICacheService cache) : ITaskService
{
    public async Task<PagedResult<TaskResponse>> GetAllAsync(Guid projectId, TaskQuery query, Guid userId, CancellationToken ct = default)
    {
        await RequireProjectAsync(projectId, userId, ct);
        var version = await cache.GetVersionAsync($"tasks:{projectId}", ct);
        var key = $"tasks:{projectId}:v{version}:s{query.Status}:a{query.AssigneeId}:o{query.SortBy}:d{query.Descending}:p{query.Page}:z{query.PageSize}";
        var cached = await cache.GetAsync<PagedResult<TaskResponse>>(key, ct); if (cached is not null) return cached;
        var page = await tasks.GetPagedAsync(projectId, query, ct);
        var result = new PagedResult<TaskResponse>(page.Items.Select(Map).ToList(), page.Page, page.PageSize, page.TotalCount);
        await cache.SetAsync(key, result, TimeSpan.FromMinutes(3), ct); return result;
    }
    public async Task<TaskResponse> GetAsync(Guid id, Guid userId, CancellationToken ct = default) => Map(await RequireAsync(id, userId, ct));
    public async Task<TaskResponse> CreateAsync(Guid projectId, CreateTaskRequest request, Guid userId, CancellationToken ct = default)
    {
        await RequireProjectAsync(projectId, userId, ct); await RequireAssigneeAsync(request.AssigneeId, ct);
        var item = new TaskItem { ProjectId = projectId, Title = request.Title.Trim(), Description = request.Description?.Trim(), Priority = request.Priority, DueDate = request.DueDate, AssigneeId = request.AssigneeId };
        await tasks.AddAsync(item, ct); await tasks.SaveChangesAsync(ct); await cache.BumpVersionAsync($"tasks:{projectId}", ct); return Map(item);
    }
    public async Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request, Guid userId, CancellationToken ct = default)
    {
        var item = await RequireAsync(id, userId, ct); await RequireAssigneeAsync(request.AssigneeId, ct);
        if (!IsValidTransition(item.Status, request.Status)) throw new ConflictException($"Cannot move a task from {item.Status} directly to {request.Status}.");
        item.Title = request.Title.Trim(); item.Description = request.Description?.Trim(); item.Status = request.Status; item.Priority = request.Priority; item.DueDate = request.DueDate; item.AssigneeId = request.AssigneeId; item.UpdatedAtUtc = DateTime.UtcNow;
        await tasks.SaveChangesAsync(ct); await cache.BumpVersionAsync($"tasks:{item.ProjectId}", ct); return Map(item);
    }
    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var item = await RequireAsync(id, userId, ct); tasks.Remove(item); await tasks.SaveChangesAsync(ct); await cache.BumpVersionAsync($"tasks:{item.ProjectId}", ct);
    }
    public async Task<IReadOnlyCollection<TaskStatusCount>> GetStatusCountsAsync(Guid projectId, Guid userId, CancellationToken ct = default)
    {
        await RequireProjectAsync(projectId, userId, ct); return await tasks.GetStatusCountsRawSqlAsync(projectId, userId, ct);
    }
    private async Task<Project> RequireProjectAsync(Guid id, Guid ownerId, CancellationToken ct) => await projects.GetByIdAsync(id, ownerId, ct) ?? throw new NotFoundException("Project not found.");
    private async Task<TaskItem> RequireAsync(Guid id, Guid ownerId, CancellationToken ct) => await tasks.GetByIdAsync(id, ownerId, ct) ?? throw new NotFoundException("Task not found.");
    private async Task RequireAssigneeAsync(Guid? assigneeId, CancellationToken ct) { if (assigneeId.HasValue && await users.GetByIdAsync(assigneeId.Value, ct) is null) throw new NotFoundException("Assignee not found."); }
    private static bool IsValidTransition(TaskStatus from, TaskStatus to) => from == to || (from, to) is (TaskStatus.Todo, TaskStatus.InProgress) or (TaskStatus.InProgress, TaskStatus.Todo) or (TaskStatus.InProgress, TaskStatus.Done) or (TaskStatus.Done, TaskStatus.InProgress);
    private static TaskResponse Map(TaskItem x) => new(x.Id, x.ProjectId, x.Title, x.Description, x.Status, x.Priority, x.DueDate, x.AssigneeId, x.CreatedAtUtc, x.UpdatedAtUtc);
}
