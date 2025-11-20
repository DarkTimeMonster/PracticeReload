using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.ViewModels.Festivals;

public class FestivalEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название фестиваля")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Display(Name = "Дата начала")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Display(Name = "Дата окончания")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [Display(Name = "URL обложки")]
    [MaxLength(500)]
    public string? CoverUrl { get; set; }
}