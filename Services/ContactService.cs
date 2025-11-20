using DAL.Interfaces;
using Domain.Entities;

namespace Services
{
    public class ContactService
    {
        private readonly IBaseStorage<ContactMessage> _storage;

        public ContactService(IBaseStorage<ContactMessage> storage)
        {
            _storage = storage;
        }

        public async Task SubmitMessageAsync(ContactMessage message, CancellationToken ct = default)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            message.CreatedAt = DateTime.UtcNow;

            await _storage.AddAsync(message, ct);
        }

        public Task<List<ContactMessage>> GetAllAsync(CancellationToken ct = default) =>
            _storage.GetAllAsync(ct);

        public Task<bool> DeleteAsync(int id, CancellationToken ct = default) =>
            _storage.DeleteAsync(id, ct);
    }
}
