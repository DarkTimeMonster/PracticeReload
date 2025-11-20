namespace Domain.ViewModels.Festivals;

public class FestivalDetailsViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    public string? City { get; set; }
    public string? Country { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public string? CoverUrl { get; set; }

    public List<ImageItem> Images { get; set; } = new();

    // Для избранного
    public bool IsFavorite { get; set; }

    public class ImageItem
    {
        public string Url { get; set; } = null!;
        public string? Alt { get; set; }
    }
}
