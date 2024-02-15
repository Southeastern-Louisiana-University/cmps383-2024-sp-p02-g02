using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Selu383.SP24.Api.Data;
using Selu383.SP24.Api.Dtos;
using Selu383.SP24.Api.Features.UserRole;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Selu383.SP24.Api.Controllers.CreateUser
{
    [Route("api/users")]
    [ApiController]
    
    //[Authorize(Roles = "Admin")] // Only admins can access this endpoint
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public UsersController(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _userManager.Users.Select(user => new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Roles = _userManager.GetRolesAsync(user).Result.ToArray()
            }).ToList();

            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDto createUserDto)
        {
            // Ensure at least one role is provided
            if (createUserDto.Roles == null || createUserDto.Roles.Count() == 0)
                return BadRequest("At least one role must be provided.");

            // Validate roles
            foreach (var roleName in createUserDto.Roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                    return BadRequest($"Role '{roleName}' does not exist.");
            }

            // Check if username is unique
            var existingUser = await _userManager.FindByNameAsync(createUserDto.UserName);
            if (existingUser != null)
                return Conflict("Username already exists.");

            // Create the user
            var user = new User { UserName = createUserDto.UserName };
            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                // Add roles to the user
                var rolesAdded = new List<string>();
                foreach (var roleName in createUserDto.Roles)
                {
                    if (await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _userManager.AddToRoleAsync(user, roleName);
                        rolesAdded.Add(roleName);
                    }
                }

                // Return the user DTO
                var userDto = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Roles = rolesAdded.ToArray()
                };
                return Ok(userDto);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }
    }
}