using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Entities;

namespace TaskTracker.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Email).HasMaxLength(320);
            entity.Property(x => x.PasswordHash).HasMaxLength(100);
        });
        modelBuilder.Entity<Project>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(120);
            entity.HasOne(x => x.Owner).WithMany(x => x.Projects).HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(200);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);
            entity.HasOne(x => x.Project).WithMany(x => x.Tasks).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Assignee).WithMany(x => x.AssignedTasks).HasForeignKey(x => x.AssigneeId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.ProjectId, x.Status });
        });
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.Property(x => x.Body).HasMaxLength(4000);
            entity.HasOne(x => x.TaskItem).WithMany(x => x.Comments).HasForeignKey(x => x.TaskItemId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Author).WithMany(x => x.Comments).HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
