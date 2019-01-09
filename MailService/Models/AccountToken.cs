using System;

namespace MailService.Models
{
    public class AccountToken
    {
        public int Id { get; set; }
        public string AccountId { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshExpires { get; set; }
    }
}
