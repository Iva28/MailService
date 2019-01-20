using MailService.DTO;
using MailService.EF;
using MailService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MailService.Services
{
    public class AccountService : IAccountService
    {
        private readonly AuthOptions _authOptions;
        private UserManager<Account> userManager;
        private SignInManager<Account> signInManager;
        private MyDbContext dbcontext;

        public AccountService(IOptions<AuthOptions> options, UserManager<Account> userManager, SignInManager<Account> signInManager, MyDbContext context)
        {
            this._authOptions = options.Value;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.dbcontext = context;
        }

        public async Task<bool> SignUp(string email, string password)
        {
            Account account = new Account() {
                UserName = email,
                Email = email,
                Sent_total = 0,
                Delivered_total = 0,
                Sent_today = 0,
                Left_today = 100,
                Delivered_today = 0
            };

            IdentityResult res = await userManager.CreateAsync (account, password);
            if (res.Succeeded)
                return true;
            return false;
        }

        public async Task<SignInResponse> SignIn(string email, string password)
        {
            var result = await signInManager.PasswordSignInAsync(email, password, false, false);
            if (result.Succeeded) {
                var account = await userManager.FindByNameAsync(email);
                SignInResponse resp = await GenerateJwtTokenAsync(account);
                if (resp != null)
                    return resp;
            }
            return null;           
        }

        public async Task<GetInfoResponse> GetInfo(string id)
        {
            Account acc = await userManager.FindByIdAsync(id);
            if (acc != null) {
                var resp = new GetInfoResponse() {
                    Sent_today = acc.Sent_today,
                    Sent_total = acc.Sent_total,
                    Left_today = acc.Left_today,
                    Delivered_today = acc.Delivered_today,
                    Delivered_total = acc.Delivered_total
                };
                return resp;
            }
            return null;
        }

        public async Task SignOut(string id)
        {
            await signInManager.SignOutAsync();

            var tokens = dbcontext.AccountTokens.Where(a => a.Account.Id == id).ToList();
            if (tokens.Count() != 0) {
                dbcontext.AccountTokens.RemoveRange(tokens);
                await dbcontext.SaveChangesAsync();
            }
        }

        public async Task<bool> SendMessage(string address, string subject, string body, string id)
        {
            Account acc = await userManager.FindByIdAsync(id);
            if (acc.Left_today > 0) {
                try {
                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress("user@gmail.com");
                    mailMessage.To.Add(new MailAddress(address));
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;

                    SmtpClient MailClient = new SmtpClient("smtp.gmail.com", 587);
                    MailClient.Credentials = new NetworkCredential("user@gmail.com", "*****");
                    MailClient.EnableSsl = true;
                    MailClient.Send(mailMessage);

                    acc.Sent_total += 1;
                    acc.Sent_today += 1;
                    acc.Left_today -= 1;
                    acc.Delivered_total += 1;
                    acc.Delivered_today += 1;

                    var msg = new Message() { Address = address, Body = body, Subject = subject, Account = acc };
                    dbcontext.Messages.Add(msg);
                    await dbcontext.SaveChangesAsync();

                    return true;
                }
                catch (SmtpFailedRecipientsException) {
                    acc.Sent_total += 1;
                    acc.Sent_today += 1;
                    acc.Left_today -= 1;
                    return false;
                }
                catch (Exception) {
                    return false;
                }
            }
            return false;
        }

        public async Task<SignInResponse> UpdateTokenAsync(string refreshToken)
        {
            var accToken = dbcontext.AccountTokens.FirstOrDefault(at => at.RefreshToken == refreshToken);
            if (accToken == null) return null;
            if (accToken.RefreshExpires <= DateTime.Now) return null;
            if (accToken.Account == null) return null;
            return await GenerateJwtTokenAsync(accToken.Account);
        }

        public async Task<SignInResponse> GenerateJwtTokenAsync(Account account)
        {
            List<Claim> claims = new List<Claim>() {
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

            SignInResponse resp = new SignInResponse() {
                Email = account.Email,
                AccessToken = tokenStr,
                RefreshToken = Guid.NewGuid().ToString()
            };

            var tokens = dbcontext.AccountTokens.Where(a => a.Account.Id == account.Id).ToList();
            if (tokens.Count() != 0)
                dbcontext.AccountTokens.RemoveRange(tokens);

            dbcontext.AccountTokens.Add(new AccountToken() {
                Account = account,
                RefreshExpires = DateTime.Now.AddMinutes(_authOptions.RefreshLifetime),
                RefreshToken = resp.RefreshToken
            });
            await dbcontext.SaveChangesAsync();
            return resp;
        }
    }
}
