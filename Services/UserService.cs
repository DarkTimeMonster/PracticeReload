using System.Security.Cryptography;
using System.Text;
using DAL;
using Domain.Entities;

namespace Services;

public class UserService
{
    private readonly UserStorage _users;

    public UserService(UserStorage users)
    {
        _users = users;
    }

    // простое хеширование пароля (для учёбы хватит)
    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes); // ABCDEF0123...
    }

    // Регистрация: возвращаем (ok, error, user)
    public async Task<(bool ok, string? error, User? user)> RegisterAsync(
        string name,
        string email,
        string password,
        CancellationToken ct = default)
    {
        // проверяем, нет ли уже такого email
        var existing = await _users.GetByEmailAsync(email, ct);
        if (existing != null)
            return (false, "Пользователь с таким email уже существует.", null);

        var user = new User
        {
            Name = name,
            Email = email,
            PwdHash = HashPassword(password),
            Role = "Customer",        
            CreatedAt = DateTime.UtcNow
        };

        await _users.AddAsync(user, ct);
        return (true, null, user);
    }

    // Логин: возвращаем (ok, error, user)
    public async Task<(bool ok, string? error, User? user)> LoginAsync(
        string email,
        string password,
        CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(email, ct);
        if (user == null)
            return (false, "Пользователь не найден.", null);

        var hash = HashPassword(password);
        if (!string.Equals(user.PwdHash, hash, StringComparison.Ordinal))
            return (false, "Неверный пароль.", null);

        user.LastLoginAt = DateTime.UtcNow;
        await _users.UpdateAsync(user, ct);

        return (true, null, user);
    }

    // Получить пользователя по Id (для профиля)
    public Task<User?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _users.GetAsync(id, ct);
    // ОБНОВИТЬ ИМЯ/EMAIL
    public async Task<(bool ok, string? error)> UpdateProfileAsync(
        int id,
        string name,
        string email,
        CancellationToken ct = default)
    {
        var user = await _users.GetAsync(id, ct);
        if (user == null)
            return (false, "Пользователь не найден.");

        // проверяем, что email не занят другим пользователем
        var existing = await _users.GetByEmailAsync(email, ct);
        if (existing != null && existing.Id != id)
            return (false, "Пользователь с таким email уже существует.");

        user.Name = name;
        user.Email = email;

        await _users.UpdateAsync(user, ct);
        return (true, null);
    }


    // СМЕНА ПАРОЛЯ
    public async Task<(bool ok, string? error)> ChangePasswordAsync(
        int id,
        string currentPassword,
        string newPassword,
        CancellationToken ct = default)
    {
        var user = await _users.GetAsync(id, ct);
        if (user == null)
            return (false, "Пользователь не найден.");

        var currentHash = HashPassword(currentPassword);
        if (!string.Equals(user.PwdHash, currentHash, StringComparison.Ordinal))
            return (false, "Текущий пароль указан неверно.");

        user.PwdHash = HashPassword(newPassword);
        await _users.UpdateAsync(user, ct);

        return (true, null);
    }


}
