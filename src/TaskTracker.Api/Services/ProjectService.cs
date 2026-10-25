using TaskTracker.Api.Common;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Entities;
using TaskTracker.Api.Repositories;

namespace TaskTracker.Api.Services;

public sealed class ProjectService(IProjectRepository projects, ICacheService cache) : IProjectService
{
    public async Task<IReadOnlyCollection<ProjectResponse>> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        var version = await cache.GetVersionAsync($"projects:{userId}", ct); var key = $"projects:{userId}:v{version}";
        var cached = await cache.GetAsync<List<ProjectResponse>>(key, ct); if (cached is not null) return cached;
        var result = (await projects.GetAllAsync(userId, ct)).Select(Map).ToList();
        await cache.SetAsync(key, result, TimeSpan.FromMinutes(5), ct); return result;
    }
    public async Task<ProjectResponse> GetAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var key = $"project:{userId}:{id}"; var cached = await cache.GetAsync<ProjectResponse>(key, ct); if (cached is not null) return cached;
        var result = Map(await RequireAsync(id, userId, ct)); await cache.SetAsync(key, result, TimeSpan.FromMinutes(5), ct); return result;
    }
    public async Task<ProjectResponse> CreateAsync(CreateProjectRequest request, Guid userId, CancellationToken ct = default)
    {
        var project = new Project { Name = request.Name.Trim(), Description = request.Description?.Trim(), OwnerId = userId };
        await projects.AddAsync(project, ct); await projects.SaveChangesAsync(ct); await cache.BumpVersionAsync($"projects:{userId}", ct); return Map(project);
    }
    public async Task<ProjectResponse> UpdateAsync(Guid id, UpdateProjectRequest request, Guid userId, CancellationToken ct = default)
    {
        var project = await RequireAsync(id, userId, ct); project.Name = request.Name.Trim(); project.Description = request.Description?.Trim(); project.UpdatedAtUtc = DateTime.UtcNow;
        await projects.SaveChangesAsync(ct); await InvalidateAsync(project, ct); return Map(project);
    }
    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var project = await RequireAsync(id, userId, ct); projects.Remove(project); await projects.SaveChangesAsync(ct); await InvalidateAsync(project, ct);
    }
    private async Task<Project> RequireAsync(Guid id, Guid userId, CancellationToken ct) => await projects.GetByIdAsync(id, userId, ct) ?? throw new NotFoundException("Project not found.");
    private async Task InvalidateAsync(Project project, CancellationToken ct) { await cache.RemoveAsync($"project:{project.OwnerId}:{project.Id}", ct); await cache.BumpVersionAsync($"projects:{project.OwnerId}", ct); await cache.BumpVersionAsync($"tasks:{project.Id}", ct); }
    private static ProjectResponse Map(Project p) => new(p.Id, p.Name, p.Description, p.CreatedAtUtc, p.UpdatedAtUtc);
}
