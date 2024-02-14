using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Selu383.SP24.Api.Controllers
{
    public class AuthenticationController: Controller
    {
        private readonly UserManager<User> userManager;

        public AuthenticationController(UserManager<User> userManager)
        {
            this.userManager = userManager;
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
    }
}
