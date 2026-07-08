
namespace Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PwdHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Новое: подтверждён ли email
    public bool EmailConfirmed { get; set; } = false;

    // Новое: код подтверждения (GUID-строка)
    public string? EmailConfirmationCode { get; set; }

    public string Role { get; set; } = "Customer";
    public ICollection<UserFavorite> Favorites { get; set; } = new List<UserFavorite>();
}
