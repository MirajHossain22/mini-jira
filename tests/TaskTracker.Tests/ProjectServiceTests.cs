using Moq;
using TaskTracker.Api.Common;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Entities;
using TaskTracker.Api.Repositories;
using TaskTracker.Api.Services;

namespace TaskTracker.Tests;

public sealed class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _projects = new();
    private readonly Mock<ICacheService> _cache = new();

    [Fact]
    public async Task Create_AssignsOwnerAndInvalidatesList()
    {
        var userId = Guid.NewGuid(); var service = new ProjectService(_projects.Object, _cache.Object);
        var result = await service.CreateAsync(new CreateProjectRequest(" Platform ", " API "), userId);
        Assert.Equal("Platform", result.Name);
        _projects.Verify(x => x.AddAsync(It.Is<Project>(p => p.OwnerId == userId && p.Name == "Platform"), It.IsAny<CancellationToken>()));
        _cache.Verify(x => x.BumpVersionAsync($"projects:{userId}", It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task Get_WhenProjectIsNotOwned_ThrowsNotFound()
    {
        _projects.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Project?)null);
        var service = new ProjectService(_projects.Object, _cache.Object);
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAll_WhenCached_DoesNotQueryRepository()
    {
        var userId = Guid.NewGuid(); var cached = new List<ProjectResponse> { new(Guid.NewGuid(), "Cached", null, DateTime.UtcNow, DateTime.UtcNow) };
        _cache.Setup(x => x.GetVersionAsync($"projects:{userId}", It.IsAny<CancellationToken>())).ReturnsAsync(3);
        _cache.Setup(x => x.GetAsync<List<ProjectResponse>>($"projects:{userId}:v3", It.IsAny<CancellationToken>())).ReturnsAsync(cached);
        var service = new ProjectService(_projects.Object, _cache.Object);
        var result = await service.GetAllAsync(userId);
        Assert.Same(cached, result); _projects.Verify(x => x.GetAllAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
