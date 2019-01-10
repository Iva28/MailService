using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace MailService.Models
{
    public class Account : IdentityUser
    {
        public int Sent_total { get; set; }
        public int Delivered_total { get; set; }
        public int Sent_today { get; set; }
        public int Left_today { get; set; }
        public int Delivered_today { get; set; }

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
