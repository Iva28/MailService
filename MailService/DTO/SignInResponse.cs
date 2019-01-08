using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MailService.DTO
{
    public class SignInResponse
    {
        public string Email { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
