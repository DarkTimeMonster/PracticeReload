using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ContactMessage
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
