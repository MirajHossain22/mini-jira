using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Api.Common;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Services;

namespace TaskTracker.Api.Controllers;

[ApiController, Authorize, Route("api/projects")]
public sealed class ProjectsController(IProjectService projects, ITaskService tasks, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProjectResponse>>> GetAll(CancellationToken ct) => Ok(await projects.GetAllAsync(currentUser.UserId, ct));
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectResponse>> Get(Guid id, CancellationToken ct) => Ok(await projects.GetAsync(id, currentUser.UserId, ct));
    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> Create(CreateProjectRequest request, CancellationToken ct)
    {
        var result = await projects.CreateAsync(request, currentUser.UserId, ct); return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectResponse>> Update(Guid id, UpdateProjectRequest request, CancellationToken ct) => Ok(await projects.UpdateAsync(id, request, currentUser.UserId, ct));
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { await projects.DeleteAsync(id, currentUser.UserId, ct); return NoContent(); }
    [HttpGet("{projectId:guid}/tasks")]
    public async Task<ActionResult<PagedResult<TaskResponse>>> GetTasks(Guid projectId, [FromQuery] TaskQuery query, CancellationToken ct) => Ok(await tasks.GetAllAsync(projectId, query, currentUser.UserId, ct));
    [HttpPost("{projectId:guid}/tasks")]
    public async Task<ActionResult<TaskResponse>> CreateTask(Guid projectId, CreateTaskRequest request, CancellationToken ct)
    {
        var result = await tasks.CreateAsync(projectId, request, currentUser.UserId, ct); return CreatedAtAction(nameof(TasksController.Get), "Tasks", new { id = result.Id }, result);
    }
    [HttpGet("{projectId:guid}/task-status-counts")]
    public async Task<ActionResult<IReadOnlyCollection<TaskStatusCount>>> StatusCounts(Guid projectId, CancellationToken ct) => Ok(await tasks.GetStatusCountsAsync(projectId, currentUser.UserId, ct));
}
