using TaskTracker.Api.Common;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Entities;

namespace TaskTracker.Api.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IProjectRepository
{
    Task<IReadOnlyCollection<Project>> GetAllAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<Project?> GetByIdAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default);
    Task AddAsync(Project project, CancellationToken cancellationToken = default);
    void Remove(Project project);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ITaskRepository
{
    Task<PagedResult<TaskItem>> GetPagedAsync(Guid projectId, TaskQuery query, CancellationToken cancellationToken = default);
    Task<TaskItem?> GetByIdAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default);
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);
    void Remove(TaskItem task);
    Task<IReadOnlyCollection<TaskStatusCount>> GetStatusCountsRawSqlAsync(Guid projectId, Guid ownerId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ICommentRepository
{
    Task<IReadOnlyCollection<Comment>> GetAllAsync(Guid taskId, Guid ownerId, CancellationToken cancellationToken = default);
    Task<Comment?> GetByIdAsync(Guid id, Guid ownerId, CancellationToken cancellationToken = default);
    Task AddAsync(Comment comment, CancellationToken cancellationToken = default);
    void Remove(Comment comment);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
