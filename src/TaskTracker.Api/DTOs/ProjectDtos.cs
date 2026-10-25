namespace TaskTracker.Api.DTOs;

public sealed record CreateProjectRequest(string Name, string? Description);
public sealed record UpdateProjectRequest(string Name, string? Description);
public sealed record ProjectResponse(Guid Id, string Name, string? Description, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);
