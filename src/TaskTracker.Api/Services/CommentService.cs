using TaskTracker.Api.Common;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Entities;
using TaskTracker.Api.Repositories;

namespace TaskTracker.Api.Services;

public sealed class CommentService(ICommentRepository comments, ITaskRepository tasks) : ICommentService
{
    public async Task<IReadOnlyCollection<CommentResponse>> GetAllAsync(Guid taskId, Guid userId, CancellationToken ct = default)
    {
        await RequireTaskAsync(taskId, userId, ct); return (await comments.GetAllAsync(taskId, userId, ct)).Select(Map).ToList();
    }
    public async Task<CommentResponse> GetAsync(Guid id, Guid userId, CancellationToken ct = default) => Map(await RequireAsync(id, userId, ct));
    public async Task<CommentResponse> CreateAsync(Guid taskId, CreateCommentRequest request, Guid userId, CancellationToken ct = default)
    {
        await RequireTaskAsync(taskId, userId, ct); var comment = new Comment { TaskItemId = taskId, AuthorId = userId, Body = request.Body.Trim() };
        await comments.AddAsync(comment, ct); await comments.SaveChangesAsync(ct); return Map(comment);
    }
    public async Task<CommentResponse> UpdateAsync(Guid id, UpdateCommentRequest request, Guid userId, CancellationToken ct = default)
    {
        var comment = await RequireAsync(id, userId, ct); if (comment.AuthorId != userId) throw new UnauthorizedException("Only the author can edit this comment.");
        comment.Body = request.Body.Trim(); comment.UpdatedAtUtc = DateTime.UtcNow; await comments.SaveChangesAsync(ct); return Map(comment);
    }
    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var comment = await RequireAsync(id, userId, ct); if (comment.AuthorId != userId) throw new UnauthorizedException("Only the author can delete this comment.");
        comments.Remove(comment); await comments.SaveChangesAsync(ct);
    }
    private async Task<TaskItem> RequireTaskAsync(Guid id, Guid userId, CancellationToken ct) => await tasks.GetByIdAsync(id, userId, ct) ?? throw new NotFoundException("Task not found.");
    private async Task<Comment> RequireAsync(Guid id, Guid userId, CancellationToken ct) => await comments.GetByIdAsync(id, userId, ct) ?? throw new NotFoundException("Comment not found.");
    private static CommentResponse Map(Comment x) => new(x.Id, x.TaskItemId, x.AuthorId, x.Body, x.CreatedAtUtc, x.UpdatedAtUtc);
}
