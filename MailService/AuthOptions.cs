using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MailService
{
    public class AuthOptions
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int AccessLifetime { get; set; }
        public int RefreshLifetime { get; set; }
        public string Key { get; set; }

        public SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
        }
    }
}
