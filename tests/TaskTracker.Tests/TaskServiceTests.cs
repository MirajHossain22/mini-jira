using Moq;
using TaskTracker.Api.Common;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Entities;
using TaskTracker.Api.Repositories;
using TaskTracker.Api.Services;
using TaskStatus = TaskTracker.Api.Entities.TaskStatus;

namespace TaskTracker.Tests;

public sealed class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _tasks = new();
    private readonly Mock<IProjectRepository> _projects = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<ICacheService> _cache = new();
    private TaskService Service => new(_tasks.Object, _projects.Object, _users.Object, _cache.Object);

    [Fact]
    public async Task Create_WhenProjectIsNotOwned_ThrowsNotFound()
    {
        _projects.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Project?)null);
        var request = new CreateTaskRequest("Task", null, TaskPriority.Medium, null, null);
        await Assert.ThrowsAsync<NotFoundException>(() => Service.CreateAsync(Guid.NewGuid(), request, Guid.NewGuid()));
        _tasks.Verify(x => x.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_WhenAssigneeDoesNotExist_ThrowsNotFound()
    {
        var project = OwnedProject(); var assigneeId = Guid.NewGuid();
        _projects.Setup(x => x.GetByIdAsync(project.Id, project.OwnerId, It.IsAny<CancellationToken>())).ReturnsAsync(project);
        _users.Setup(x => x.GetByIdAsync(assigneeId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => Service.CreateAsync(project.Id, new CreateTaskRequest("Task", null, TaskPriority.High, null, assigneeId), project.OwnerId));
    }

    [Fact]
    public async Task Update_CannotSkipFromTodoToDone()
    {
        var item = OwnedTask(TaskStatus.Todo);
        _tasks.Setup(x => x.GetByIdAsync(item.Id, item.Project.OwnerId, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        var request = new UpdateTaskRequest("Task", null, TaskStatus.Done, TaskPriority.Medium, null, null);
        await Assert.ThrowsAsync<ConflictException>(() => Service.UpdateAsync(item.Id, request, item.Project.OwnerId));
        _tasks.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_WithValidTransition_SavesAndInvalidatesTaskLists()
    {
        var item = OwnedTask(TaskStatus.InProgress);
        _tasks.Setup(x => x.GetByIdAsync(item.Id, item.Project.OwnerId, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        var request = new UpdateTaskRequest("Done task", null, TaskStatus.Done, TaskPriority.High, null, null);
        var result = await Service.UpdateAsync(item.Id, request, item.Project.OwnerId);
        Assert.Equal(TaskStatus.Done, result.Status);
        _tasks.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()));
        _cache.Verify(x => x.BumpVersionAsync($"tasks:{item.ProjectId}", It.IsAny<CancellationToken>()));
    }

    private static Project OwnedProject() => new() { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), Name = "Project" };
    private static TaskItem OwnedTask(TaskStatus status)
    {
        var project = OwnedProject();
        return new TaskItem { Id = Guid.NewGuid(), ProjectId = project.Id, Project = project, Title = "Task", Status = status };
    }
}
