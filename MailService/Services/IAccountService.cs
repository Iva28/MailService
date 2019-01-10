using MailService.DTO;
using System.Threading.Tasks;

namespace MailService.Services
{
    public interface IAccountService
    {
        Task<bool> SignUp(string email, string password);
        Task<SignInResponse> SignIn(string email, string password);
        Task<GetInfoResponse> GetInfo(string id);
        Task<bool> SendMessage(string address, string subject, string body, string id);
        Task<SignInResponse> UpdateTokenAsync(string refreshToken);
        Task SignOut(string id);
    }
}
