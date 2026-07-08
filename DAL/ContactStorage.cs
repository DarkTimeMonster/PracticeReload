using DAL.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL;

public class ContactStorage : IBaseStorage<ContactMessage>
{
    private readonly GastroFestDbContext _db;

    public ContactStorage(GastroFestDbContext db)
    {
        _db = db;
    }

    // CREATE
    public async Task<ContactMessage> AddAsync(ContactMessage entity, CancellationToken ct = default)
    {
        await _db.ContactMessages.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    // UPDATE
    public async Task<ContactMessage> UpdateAsync(ContactMessage entity, CancellationToken ct = default)
    {
        _db.ContactMessages.Update(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    // DELETE
    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var msg = await _db.ContactMessages.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (msg == null)
            return false;

        _db.ContactMessages.Remove(msg);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // GET ONE
    public Task<ContactMessage?> GetAsync(int id, CancellationToken ct = default) =>
        _db.ContactMessages.FirstOrDefaultAsync(x => x.Id == id, ct);

    // GET ALL
    public Task<List<ContactMessage>> GetAllAsync(CancellationToken ct = default) =>
        _db.ContactMessages
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);
}
