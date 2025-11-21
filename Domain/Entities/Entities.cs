using System;
using System.Collections.Generic;

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

public class Festival
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public decimal? Latitude { get; set; }   // NUMERIC(9,6)
    public decimal? Longitude { get; set; }  // NUMERIC(9,6)
    public string? CoverUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<FestivalImage> Images { get; set; } = new List<FestivalImage>();
    public ICollection<UserFavorite> Favorites { get; set; } = new List<UserFavorite>();
}

public class FestivalImage
{
    public int Id { get; set; }
    public int FestivalId { get; set; }
    public string Url { get; set; } = null!;
    public string? Alt { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }

    public Festival? Festival { get; set; }
}

public class UserFavorite
{
    public int UserId { get; set; } 
    public int FestivalId { get; set; }
    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
    public Festival? Festival { get; set; }
}

public class ContactMessage
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Message { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}
