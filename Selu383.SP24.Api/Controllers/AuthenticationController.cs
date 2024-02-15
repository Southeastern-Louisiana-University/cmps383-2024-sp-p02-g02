using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Selu383.SP24.Api.Features.UserRole;
using Selu383.SP24.Api.Data;
using Selu383.SP24.Api.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

namespace Selu383.SP24.Api.Controllers.AuthenticationController
{

    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly DbSet<UserRole> _userRoles;
        private readonly DataContext _dataContext;
        public AuthenticationController(UserManager<User> userManager, SignInManager<User> signInManager, DataContext datacontext)
        {
            this._dataContext = datacontext;
            _userRoles = datacontext.Set<UserRole>();
            _userManager = userManager;
            _signInManager = signInManager;

            
        }

        [HttpPost("login")]
        
        public async Task<ActionResult<UserDto>> Login(LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null)
            {
                return BadRequest("Invalid username or password");
            }

            var result = await _signInManager.PasswordSignInAsync(user, dto.Password, false, false);
            if (!result.Succeeded)
            {
                return BadRequest("Invalid username or password");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Roles = roles.ToArray()
            };

            return Ok(userDto);
        }

        [Authorize]
        [HttpGet("me")]

        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Roles = roles.ToArray()
            };

            return Ok(userDto);
        }

        [Authorize]
        [HttpPost("logout")]
      

        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }
    }
}