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

    // РЕГИСТРАЦИЯ: возвращаем (ok, error, user)
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
            CreatedAt = DateTime.UtcNow,

            // новое: email ещё НЕ подтверждён
            EmailConfirmed = false,
            // новое: генерируем уникальный код для подтверждения
            EmailConfirmationCode = Guid.NewGuid().ToString("N")
        };

        await _users.AddAsync(user, ct);
        return (true, null, user);
    }

    // ЛОГИН: возвращаем (ok, error, user)
    public async Task<(bool ok, string? error, User? user)> LoginAsync(
        string email,
        string password,
        CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(email, ct);
        if (user == null)
            return (false, "Пользователь не найден.", null);

        // новая проверка подлинности email
        if (!user.EmailConfirmed)
            return (false, "Ваш email ещё не подтверждён. Проверьте почту.", null);

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

    // ПОДТВЕРЖДЕНИЕ EMAIL ПО КОДУ
    public async Task<(bool ok, string? error, User? user)> ConfirmEmailAsync(
        int userId,
        string code,
        CancellationToken ct = default)
    {
        var user = await _users.GetAsync(userId, ct);
        if (user == null)
            return (false, "Пользователь не найден.", null);

        if (user.EmailConfirmed)
            return (false, "Email уже подтверждён.", user);

        if (string.IsNullOrEmpty(user.EmailConfirmationCode) ||
            !string.Equals(user.EmailConfirmationCode, code, StringComparison.Ordinal))
            return (false, "Некорректный код подтверждения.", null);

        user.EmailConfirmed = true;
        user.EmailConfirmationCode = null;

        await _users.UpdateAsync(user, ct);

        return (true, null, user);
    }
}
