using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.Entities;

namespace TaskTracker.Api.Repositories;

public sealed class CommentRepository(AppDbContext db) : ICommentRepository
{
    public async Task<IReadOnlyCollection<Comment>> GetAllAsync(Guid taskId, Guid ownerId, CancellationToken ct = default) =>
        await db.Comments.AsNoTracking().Where(x => x.TaskItemId == taskId && x.TaskItem.Project.OwnerId == ownerId).OrderBy(x => x.CreatedAtUtc).ToListAsync(ct);
    public Task<Comment?> GetByIdAsync(Guid id, Guid ownerId, CancellationToken ct = default) =>
        db.Comments.Include(x => x.TaskItem).ThenInclude(x => x.Project).SingleOrDefaultAsync(x => x.Id == id && x.TaskItem.Project.OwnerId == ownerId, ct);
    public Task AddAsync(Comment comment, CancellationToken ct = default) => db.Comments.AddAsync(comment, ct).AsTask();
    public void Remove(Comment comment) => db.Comments.Remove(comment);
    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
