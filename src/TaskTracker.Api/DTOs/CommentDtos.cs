namespace TaskTracker.Api.DTOs;

public sealed record CreateCommentRequest(string Body);
public sealed record UpdateCommentRequest(string Body);
public sealed record CommentResponse(Guid Id, Guid TaskItemId, Guid AuthorId, string Body, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);
