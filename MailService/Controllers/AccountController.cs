using MailService.DTO;
using MailService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MailService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        private IAccountService accountService;

        public AccountController(IAccountService accountService)
        {
            this.accountService = accountService;
        }

        [HttpPost]
        public async Task<IActionResult> SignUp([FromBody]SignUpRequest model)
        {
            var success = await accountService.SignUp(model.Email, model.Password);
            if (success)
                return Ok();
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn([FromBody]SignInRequest model)
        {
            var resp = await accountService.SignIn(model.Email, model.Password);
            if (resp != null)
                return new JsonResult(resp);
            return ValidationProblem();
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetInfo()
        {
            var id = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var resp = await accountService.GetInfo(id);
            if (resp != null)
                return new JsonResult(resp);
            return BadRequest();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SendMsg([FromBody]SendMessageRequest model)
        {
            var id = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var success = await accountService.SendMessage(model.Address, model.Subject, model.Body, id);
            if (success) {
                return Ok(await accountService.GetInfo(id));
            }
            return BadRequest();

            //https://myaccount.google.com/lesssecureapps -- allow less secure apps: OFF => ON
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SignOut()
        {
            var id = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            await accountService.SignOut(id);
            return Ok();          
        }
    }
}
