using Domain.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL;

public class UserStorage : IBaseStorage<User>
{
    private readonly GastroFestDbContext _db;

    public UserStorage(GastroFestDbContext db)
    {
        _db = db;
    }

    public async Task<User> AddAsync(User entity, CancellationToken ct = default)   
    {
        await _db.Users.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<User> UpdateAsync(User entity, CancellationToken ct = default)
    {
        _db.Users.Update(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (user == null)
            return false;

        _db.Users.Remove(user);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public Task<User?> GetAsync(int id, CancellationToken ct = default) =>
        _db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<User>> GetAllAsync(CancellationToken ct = default) =>
        _db.Users
            .OrderBy(x => x.Id)
            .ToListAsync(ct);

    // Нужен для UserService
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
}
