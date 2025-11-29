using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
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
}
