namespace MailService.DTO
{
    public class SignInResponse
    {
        public string Email { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
