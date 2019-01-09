using MailService.DTO;
using MailService.EF;
using MailService.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;

namespace MailService.Services
{
    public class AccountService : IAccountService
    {
        private readonly AuthOptions _authOptions;

        private MyDbContext context;

        public AccountService(IOptions<AuthOptions> options, MyDbContext context)
        {
            this._authOptions = options.Value;
            this.context = context;
        }

        public SignInResponse SignIn(Account account)
        {
            return GenerateJwtToken(account);
        }      

        public void SignOut(string id)
        {
            var tokens = context.AccountTokens.Where(a => a.AccountId == id).ToList();
            if (tokens.Count() != 0)
                context.AccountTokens.RemoveRange(tokens);
        }

        public SignInResponse GenerateJwtToken(Account account)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, account.Email),
                new Claim(ClaimTypes.NameIdentifier, account.Id),
            };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimTypes.NameIdentifier);

            JwtSecurityToken token = new JwtSecurityToken(
                   issuer: _authOptions.Issuer,
                   audience: _authOptions.Audience,
                   claims: claimsIdentity.Claims,
                   expires: DateTime.Now.AddMinutes(_authOptions.AccessLifetime),
                   signingCredentials: new SigningCredentials(_authOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
            );
            string tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
            SignInResponse resp = new SignInResponse()
            {
                Email = account.Email,
                AccessToken = tokenStr,
                RefreshToken = Guid.NewGuid().ToString()
            };

            var tokens = context.AccountTokens.Where(a => a.AccountId == account.Id).ToList();
            if (tokens.Count()!=0)
                context.AccountTokens.RemoveRange(tokens);

            context.AccountTokens.Add(new AccountToken() {
                AccountId = account.Id,
                RefreshExpires = DateTime.Now.AddMinutes(_authOptions.RefreshLifetime),
                RefreshToken = resp.RefreshToken });
            context.SaveChanges();

            return resp;
        }

        public bool SendMessage(string address, string subject, string body)
        {
            try
            {
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("user@gmail.com");
                mailMessage.To.Add(new MailAddress(address));
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                SmtpClient MailClient = new SmtpClient("smtp.gmail.com", 587);
                MailClient.Credentials = new NetworkCredential("user@gmail.com", "*****");
                MailClient.EnableSsl = true;
                MailClient.Send(mailMessage);
                return true;
            }
            catch (Exception ex) {
                if (ex.GetType() == typeof(SmtpFailedRecipientException))
                {
                    //
                }

                if (ex.GetType() == typeof(SmtpException))
                {
                    //
                }
                return false;
            }
        }
    }
}
