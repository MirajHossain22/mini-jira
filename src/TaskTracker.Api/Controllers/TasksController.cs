using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Services;

namespace TaskTracker.Api.Controllers;

[ApiController, Authorize, Route("api/tasks")]
public sealed class TasksController(ITaskService tasks, ICommentService comments, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskResponse>> Get(Guid id, CancellationToken ct) => Ok(await tasks.GetAsync(id, currentUser.UserId, ct));
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskResponse>> Update(Guid id, UpdateTaskRequest request, CancellationToken ct) => Ok(await tasks.UpdateAsync(id, request, currentUser.UserId, ct));
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { await tasks.DeleteAsync(id, currentUser.UserId, ct); return NoContent(); }
    [HttpGet("{taskId:guid}/comments")]
    public async Task<ActionResult<IReadOnlyCollection<CommentResponse>>> GetComments(Guid taskId, CancellationToken ct) => Ok(await comments.GetAllAsync(taskId, currentUser.UserId, ct));
    [HttpPost("{taskId:guid}/comments")]
    public async Task<ActionResult<CommentResponse>> CreateComment(Guid taskId, CreateCommentRequest request, CancellationToken ct)
    {
        var result = await comments.CreateAsync(taskId, request, currentUser.UserId, ct); return CreatedAtAction(nameof(CommentsController.Get), "Comments", new { id = result.Id }, result);
    }
}
