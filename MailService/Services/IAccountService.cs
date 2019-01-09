using MailService.DTO;
using MailService.Models;

namespace MailService.Services
{
    public interface IAccountService
    {
        SignInResponse SignIn(Account account);
        void SignOut(string id);
        bool SendMessage(string address, string subject, string text);
    }
}
