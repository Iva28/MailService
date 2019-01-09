using MailService.DTO;
using MailService.Models;
using MailService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MailService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
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
            return ValidationProblem();          
        }

        [HttpGet]
        public async Task<IActionResult> GetInfo()
        {
            Account acc = await _userManager.FindByIdAsync(_userManager.GetUserId(HttpContext.User));
            if (acc != null) {
                var resp = new GetInfoResponse() {
                    Sent_total = acc.Sent_total,
                    Delivered_total = acc.Delivered_total,
                    Sent_today = acc.Sent_today,
                    Left_today = acc.Left_today,
                    Delivered_today = acc.Delivered_today
                };
                return new JsonResult(resp);
            }
            return Unauthorized();
        }

        [HttpPost]
        public async Task<IActionResult> SendMsg([FromBody]SendMessageRequest model)
        {
            Account acc = await _userManager.FindByIdAsync(_userManager.GetUserId(HttpContext.User)); 
            if (acc == null)
                return Unauthorized();
            var success = _accountService.SendMessage(model.Address, model.Subject, model.Body);
            if (success)
                return Ok();
            return BadRequest();

            //https://myaccount.google.com/lesssecureapps -- allow less secure apps: OFF => ON
        }

        [HttpGet]
        public async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();

            var accId = _userManager.GetUserId(HttpContext.User);
            Account acc = await _userManager.FindByIdAsync(accId);
            if (acc!= null) {
                _accountService.SignOut(accId);
                return Ok();
            }
            return BadRequest();
        }
    }
}
