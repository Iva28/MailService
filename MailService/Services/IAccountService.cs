using MailService.DTO;
using MailService.Models;

namespace MailService.Services
{
    public interface IAccountService
    {
        SignInResponse SignIn(Account account);
        void SignOut(int id);
        Account GetInfo(int id);
        SignInResponse UpdateToken(string refreshToken);
    }
}
