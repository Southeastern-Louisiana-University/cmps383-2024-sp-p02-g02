using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Selu383.SP24.Api.Features.UserRole;
using System.Reflection.Metadata.Ecma335;

namespace Selu383.SP24.Api.Controllers
{
    [ApiController]
    [Route("/Authentication")]
    public class AuthenticationController : Controller
    {
        private readonly IAuthenticationService authenticationService;
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;

        public AuthenticationController(IAuthenticationService authenticationService,UserManager<User> userManager, SignInManager<User> signInManager)
        {
            this.userManager = userManager;
            this.authenticationService = authenticationService; 
            this.signInManager = signInManager;
        }
        public IActionResult Index()
        {
            userManager.CreateAsync(new User
            {
                Email = string.Empty,
            }, "Password123"
            );
            return View();
        }

        [HttpPost]
        [Route("{login}")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await userManager.FindByNameAsync(loginDto.UserName ?? "");

            if (user.UserName == null)
            {
                return BadRequest();
            }

            var result = await signInManager.PasswordSignInAsync(
                loginDto.UserName,
                loginDto.Password,
                false,
                false);

            if (!result.Succeeded)
            {
                return BadRequest("Username or password incomplete");
            }

            return Ok(userManager);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok();
        }

        public class LoginDto
        {
            public string UserName { get; set;}
            public string Password { get; set;}
        }
        

    



        
    }
}
