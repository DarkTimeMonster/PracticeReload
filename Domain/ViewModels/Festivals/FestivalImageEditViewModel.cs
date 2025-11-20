using System.ComponentModel.DataAnnotations;

namespace Domain.ViewModels.Festivals;

public class FestivalImageEditViewModel
{
    public int Id { get; set; }

    [Required]
    public int FestivalId { get; set; }

    [Required, Url]
    [Display(Name = "URL картинки")]
    public string Url { get; set; } = null!;

    [Display(Name = "Alt-текст")]
    public string? Alt { get; set; }

    [Display(Name = "Порядок")]
    public int SortOrder { get; set; }
}
