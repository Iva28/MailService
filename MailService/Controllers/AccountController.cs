using MailService.DTO;
using MailService.Models;
using MailService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MailService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private IAccountService _accountService;

        private UserManager<Account> _userManager;
        private SignInManager<Account> _signInManager;

        public AccountController(UserManager<Account> userManager, SignInManager<Account> signInManager, IAccountService accountService)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._accountService = accountService;
        }

        [HttpPost]
        public async Task<IActionResult> SignUp([FromBody]SignUpRequest model)
        {
            Account account = new Account()
            {
                UserName = model.Email,
                Email = model.Email,
                Sent_total = 0,
                Delivered_total = 0,
                Sent_today = 0,
                Left_today = 100,
                Delivered_today = 0
            };

            IdentityResult res = await _userManager.CreateAsync(account, model.Password);
            if (res.Succeeded)
                return Ok();
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn([FromBody]SignInRequest model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (result.Succeeded) {
                var account = await _userManager.FindByNameAsync(model.Email);
                SignInResponse resp = _accountService.SignIn(account);
                if (resp != null)
                    return new JsonResult(resp);
            }
            return NotFound();


            //_userManager.RemoveAuthenticationTokenAsync(account, "MyApp", "RefreshToken");
            //_userManager.GenerateUserTokenAsync(account, "MyApp", "RefreshToken");
            //_userManager.SetAuthenticationTokenAsync(account, "MyApp", "RefreshToken", newRefreshToken);
        }
    }
}
