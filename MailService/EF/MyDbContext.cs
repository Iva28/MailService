using MailService.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MailService.EF
{
    public class MyDbContext : IdentityDbContext<Account>
    {
        public MyDbContext(DbContextOptions opts) : base(opts) { }

       // public DbSet<AccountToken> AccountTokens { get; set; }
    }
}
