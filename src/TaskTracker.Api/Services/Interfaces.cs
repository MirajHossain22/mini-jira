using TaskTracker.Api.Common;
using TaskTracker.Api.DTOs;

namespace TaskTracker.Api.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
public interface IProjectService
{
    Task<IReadOnlyCollection<ProjectResponse>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ProjectResponse> GetAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<ProjectResponse> CreateAsync(CreateProjectRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<ProjectResponse> UpdateAsync(Guid id, UpdateProjectRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
public interface ITaskService
{
    Task<PagedResult<TaskResponse>> GetAllAsync(Guid projectId, TaskQuery query, Guid userId, CancellationToken cancellationToken = default);
    Task<TaskResponse> GetAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<TaskResponse> CreateAsync(Guid projectId, CreateTaskRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TaskStatusCount>> GetStatusCountsAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
}
public interface ICommentService
{
    Task<IReadOnlyCollection<CommentResponse>> GetAllAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);
    Task<CommentResponse> GetAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<CommentResponse> CreateAsync(Guid taskId, CreateCommentRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<CommentResponse> UpdateAsync(Guid id, UpdateCommentRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
public interface ITokenService { string Create(Guid userId, string email); }
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<long> GetVersionAsync(string scope, CancellationToken cancellationToken = default);
    Task BumpVersionAsync(string scope, CancellationToken cancellationToken = default);
}
