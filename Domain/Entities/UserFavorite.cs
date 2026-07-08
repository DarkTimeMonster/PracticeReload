using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
   public class UserFavorite
{
    public int UserId { get; set; } 
    public int FestivalId { get; set; }
    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
    public Festival? Festival { get; set; }
}
}
