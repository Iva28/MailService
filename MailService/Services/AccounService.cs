using MailService.DTO;
using MailService.EF;
using MailService.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
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
            if (account != null)
                return GenerateJwtToken(account);
            return null;
        }

        public Account GetInfo(int id)
        {
            throw new System.NotImplementedException();
        }

        public SignInResponse UpdateToken(string refreshToken)
        {
            throw new System.NotImplementedException();
        }

        public void SignOut(int id)
        {
            throw new System.NotImplementedException();
        }

        public SignInResponse GenerateJwtToken(Account account)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, account.Email),
                new Claim(ClaimTypes.NameIdentifier, account.Id),
            };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

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
            return resp;
        }
    }
}
