using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Selu383.SP24.Api.Data;
using Selu383.SP24.Api.Features.Hotels;
using Selu383.SP24.Api.Features.UserRole;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add identity and entity framework stores
builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<DataContext>();

// Add services to the container.
builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    await db.Database.MigrateAsync();

    var hotels = db.Set<Hotel>();

    if (!await hotels.AnyAsync())
    {
        for (int i = 0; i < 6; i++)
        {
            db.Set<Hotel>()
                .Add(new Hotel
                {
                    Name = "Hammond " + i,
                    Address = "1234 Place st"
                });
        }

        await db.SaveChangesAsync();
    }
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    var users = db.Set<Role>();
    if (!await users.AnyAsync())
    {
        await roleManager.CreateAsync(new Role { Name = "Admin" });
    }

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roles = db.Set<User>();
    if (!await users.AnyAsync())
    {
        await userManager.CreateAsync(new User { UserName = "galkadi" }, "");
    }

    async void SeedData(ModelBuilder modelBuilder, UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        // Seed roles using RoleManager
        await SeedRoles(roleManager);

        // Seed users using UserManager
        await SeedUsers(userManager);

        SeedData(modelBuilder, userManager, roleManager);
    }

    async Task SeedRoles(RoleManager<Role> roleManager)
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

    async Task SeedUsers(UserManager<User> userManager)
    {
        var adminUser = new User { UserName = "galkadi" };
        var bobUser = new User { UserName = "bob" };
        var sueUser = new User { UserName = "sue" };

        // Create users using UserManager
        await CreateUserWithRole(userManager, adminUser, "Admin", "Password123!");
        await CreateUserWithRole(userManager, bobUser, "User", "Password123!");
        await CreateUserWithRole(userManager, sueUser, "User", "Password123!");
    }

    async Task CreateUserWithRole(UserManager<User> userManager, User user, string roleName, string password)
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

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}

//see: https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0
// Hi 383 - this is added so we can test our web project automatically
public partial class Program { }
