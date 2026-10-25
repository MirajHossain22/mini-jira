using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Common;
using TaskTracker.Api.Data;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Entities;

namespace TaskTracker.Api.Repositories;

public sealed class TaskRepository(AppDbContext db) : ITaskRepository
{
    public async Task<PagedResult<TaskItem>> GetPagedAsync(Guid projectId, TaskQuery query, CancellationToken ct = default)
    {
        var source = db.Tasks.AsNoTracking().Where(x => x.ProjectId == projectId);
        if (query.Status.HasValue) source = source.Where(x => x.Status == query.Status);
        if (query.AssigneeId.HasValue) source = source.Where(x => x.AssigneeId == query.AssigneeId);
        var total = await source.CountAsync(ct);
        source = (query.SortBy.ToLowerInvariant(), query.Descending) switch
        {
            ("title", false) => source.OrderBy(x => x.Title),
            ("title", true) => source.OrderByDescending(x => x.Title),
            ("updatedat", false) => source.OrderBy(x => x.UpdatedAtUtc),
            ("updatedat", true) => source.OrderByDescending(x => x.UpdatedAtUtc),
            ("duedate", false) => source.OrderBy(x => x.DueDate),
            ("duedate", true) => source.OrderByDescending(x => x.DueDate),
            ("priority", false) => source.OrderBy(x => x.Priority),
            ("priority", true) => source.OrderByDescending(x => x.Priority),
            ("status", false) => source.OrderBy(x => x.Status),
            ("status", true) => source.OrderByDescending(x => x.Status),
            (_, false) => source.OrderBy(x => x.CreatedAtUtc),
            _ => source.OrderByDescending(x => x.CreatedAtUtc)
        };
        var items = await source.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new(items, query.Page, query.PageSize, total);
    }

    public Task<TaskItem?> GetByIdAsync(Guid id, Guid ownerId, CancellationToken ct = default) =>
        db.Tasks.Include(x => x.Project).SingleOrDefaultAsync(x => x.Id == id && x.Project.OwnerId == ownerId, ct);
    public Task AddAsync(TaskItem task, CancellationToken ct = default) => db.Tasks.AddAsync(task, ct).AsTask();
    public void Remove(TaskItem task) => db.Tasks.Remove(task);
    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);

    public async Task<IReadOnlyCollection<TaskStatusCount>> GetStatusCountsRawSqlAsync(Guid projectId, Guid ownerId, CancellationToken ct = default)
    {
        var connection = db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT t."Status", COUNT(*)::int AS "Count"
            FROM "Tasks" t
            INNER JOIN "Projects" p ON p."Id" = t."ProjectId"
            WHERE t."ProjectId" = @projectId AND p."OwnerId" = @ownerId
            GROUP BY t."Status"
            ORDER BY t."Status"
            """;
        var projectParam = command.CreateParameter(); projectParam.ParameterName = "projectId"; projectParam.Value = projectId;
        var ownerParam = command.CreateParameter(); ownerParam.ParameterName = "ownerId"; ownerParam.Value = ownerId;
        command.Parameters.Add(projectParam); command.Parameters.Add(ownerParam);
        var result = new List<TaskStatusCount>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            result.Add(new(Enum.Parse<Entities.TaskStatus>(reader.GetString(0)), reader.GetInt32(1)));
        return result;
    }
}
