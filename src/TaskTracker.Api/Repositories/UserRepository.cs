using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.Entities;

namespace TaskTracker.Api.Repositories;

public sealed class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        db.Users.SingleOrDefaultAsync(x => x.Email == email, ct);
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) => db.Users.FindAsync([id], ct).AsTask();
    public Task AddAsync(User user, CancellationToken ct = default) => db.Users.AddAsync(user, ct).AsTask();
    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
