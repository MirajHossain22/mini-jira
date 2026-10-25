using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.Entities;

namespace TaskTracker.Api.Repositories;

public sealed class ProjectRepository(AppDbContext db) : IProjectRepository
{
    public async Task<IReadOnlyCollection<Project>> GetAllAsync(Guid ownerId, CancellationToken ct = default) =>
        await db.Projects.AsNoTracking().Where(x => x.OwnerId == ownerId).OrderBy(x => x.Name).ToListAsync(ct);
    public Task<Project?> GetByIdAsync(Guid id, Guid ownerId, CancellationToken ct = default) =>
        db.Projects.SingleOrDefaultAsync(x => x.Id == id && x.OwnerId == ownerId, ct);
    public Task AddAsync(Project project, CancellationToken ct = default) => db.Projects.AddAsync(project, ct).AsTask();
    public void Remove(Project project) => db.Projects.Remove(project);
    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
