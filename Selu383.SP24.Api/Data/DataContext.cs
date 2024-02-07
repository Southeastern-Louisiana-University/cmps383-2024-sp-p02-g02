using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Selu383.SP24.Api.Features.UserRole;
using System.Reflection.Emit;

namespace Selu383.SP24.Api.Data;

public class DataContext : IdentityDbContext<User, Role, int, IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>

   
    public DataContext(DbContextOptions<DataContext> options, UserManager<User> userManager, RoleManager<Role> roleManager) : base(options)
    {


    }

    public DataContext()
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);

        var userRoleBuilder = modelBuilder.Entity<UserRole>();

        userRoleBuilder.HasKey(x => new { x.UserId, x.RoleId });

        userRoleBuilder.HasOne(navigationExpression: x => x.Role)
            .WithMany(navigationExpression: x => x.UserRoles)
            .HasForeignKey(x => x.RoleId);

        userRoleBuilder.HasOne(navigationExpression: x => x.User)
            .WithMany(navigationExpression: x => x.UserRoles)
            .HasForeignKey(x => x.UserId);

        SeedData(modelBuilder, userManager, roleManager);
    }

    //private void SeedData(ModelBuilder modelBuilder)
    //{
    //    modelBuilder.Entity<Role>().HasData(
    //        new Role { Id = 1, Name = "Admin", NormalizedName = "ADMIN" },
    //        new Role { Id = 2, Name = "User", NormalizedName = "USER" });

    //    modelBuilder.Entity<User>().HasData(
    //        new User { Id = 1, UserName = "admin", NormalizedUserName = "ADMIN" },
    //        new User { Id = 2, UserName = "user", NormalizedUserName = "USER" });

    //    modelBuilder.Entity<UserRole>().HasData(
    //        new UserRole { UserId = 1, RoleId = 1 },
    //        new UserRole { UserId = 2, RoleId = 2 });
    
    private async void SeedData(ModelBuilder modelBuilder, UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        // Seed roles using RoleManager
        await SeedRoles(roleManager);

        // Seed users using UserManager
        await SeedUsers(userManager);
    }

    private async Task SeedRoles(RoleManager<Role> roleManager)
    {
        string[] roleNames = { "Admin", "User" };

        foreach (var roleName in roleNames)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                // Create the role using RoleManager
                var role = new Role { Name = roleName, NormalizedName = roleName.ToUpper() };
                await roleManager.CreateAsync(role);
            }
        }
    }

    private async Task SeedUsers(UserManager<User> userManager)
    {
        var adminUser = new User { UserName = "galkadi"};
        var bobUser = new User { UserName = "bob"};
        var sueUser = new User { UserName = "sue"};

        // Create users using UserManager
        await CreateUserWithRole(userManager, adminUser, "Admin", "Password123!");
        await CreateUserWithRole(userManager, bobUser, "User", "Password123!");
        await CreateUserWithRole(userManager, sueUser, "User", "Password123!");
    }

    private async Task CreateUserWithRole(UserManager<User> userManager, User user, string roleName, string password)
    {
        var userExists = await userManager.FindByNameAsync(user.UserName);
        if (userExists == null)
        {
            // Create the user using UserManager
            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // Assign the role using UserManager
                await userManager.AddToRoleAsync(user, roleName);
            }
        }
    }
}
