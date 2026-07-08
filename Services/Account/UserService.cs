using System.Security.Cryptography;
using System.Text;
using Domain.Entities;
using DAL;

namespace Services.Account;

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

            EmailConfirmed = false,
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

        if (!user.EmailConfirmed)
            return (false, "Ваш email ещё не подтверждён. Проверьте почту.", null);

        var hash = HashPassword(password);
        if (!string.Equals(user.PwdHash, hash, StringComparison.Ordinal))
            return (false, "Неверный пароль.", null);

        user.LastLoginAt = DateTime.UtcNow;
        await _users.UpdateAsync(user, ct);

        return (true, null, user);
    }

    // ПОЛУЧИТЬ ПОЛЬЗОВАТЕЛЯ ПО ID (для профиля)
    public Task<User?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _users.GetAsync(id, ct);

    // ОБНОВЛЕНИЕ ПРОФИЛЯ (ИМЯ + EMAIL)
    public async Task<(bool ok, string? error)> UpdateProfileAsync(
        int id,
        string name,
        string email,
        CancellationToken ct = default)
    {
        var user = await _users.GetAsync(id, ct);
        if (user == null)
            return (false, "Пользователь не найден.");

        // если email меняется — проверяем уникальность
        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _users.GetByEmailAsync(email, ct);
            if (existing != null && existing.Id != id)
                return (false, "Пользователь с таким email уже существует.");
        }

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

    public async Task<User> GetOrCreateFromGoogleAsync(
    string email,
    string? name,
    CancellationToken ct = default)
    {
        // пробуем найти пользователя по email
        var user = await _users.GetByEmailAsync(email, ct);
        if (user != null)
        {
            // обновим имя, если из Google пришло что-то более осмысленное
            if (!string.IsNullOrWhiteSpace(name) && user.Name != name)
            {
                user.Name = name;
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _users.UpdateAsync(user, ct);

            return user;
        }

        // если пользователя ещё нет — создаём его
        user = new User
        {
            Name = string.IsNullOrWhiteSpace(name) ? email : name,
            Email = email,
            // случайный пароль, чтобы поле не было пустым
            PwdHash = HashPassword(Guid.NewGuid().ToString("N")),
            Role = "Customer",
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,

            // раз пришли из Google, считаем email подтверждённым
            EmailConfirmed = true,
            EmailConfirmationCode = null
        };

        await _users.AddAsync(user, ct);
        return user;
    }

}
