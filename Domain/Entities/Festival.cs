using Domain.Entities;

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