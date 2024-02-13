using Microsoft.AspNetCore.Identity;

namespace Selu383.SP24.Api.Features.UserRole
{
    public class User : IdentityUser<int>
    {
        public int UserId { get; set; }
        public string UserName { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

    public class Role : IdentityRole<int>
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

    // The joining entity
    public class UserRole : IdentityUserRole<int> { 
        public int UserId { get; set; }
        public User User { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}
