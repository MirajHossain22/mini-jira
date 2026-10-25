using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Services;

namespace TaskTracker.Api.Controllers;

[ApiController, Authorize, Route("api/comments")]
public sealed class CommentsController(ICommentService comments, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CommentResponse>> Get(Guid id, CancellationToken ct) => Ok(await comments.GetAsync(id, currentUser.UserId, ct));
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CommentResponse>> Update(Guid id, UpdateCommentRequest request, CancellationToken ct) => Ok(await comments.UpdateAsync(id, request, currentUser.UserId, ct));
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { await comments.DeleteAsync(id, currentUser.UserId, ct); return NoContent(); }
}
