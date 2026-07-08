using System.Collections.Generic;
using Domain.Entities;

namespace Domain.ViewModels.Festivals
{
    public class FestivalListViewModel
    {
        public List<Festival> Festivals { get; set; } = new();

        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        // Фильтры
        public string? Search { get; set; }
        public string? City { get; set; }
        /// <summary>
        /// "currentMonth" | "nextMonth" | null/"" (любые даты)
        /// </summary>
        public string? DateFilter { get; set; }
    }
}
