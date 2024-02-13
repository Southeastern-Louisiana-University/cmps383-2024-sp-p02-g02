using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Selu383.SP24.Api.Data;
using Selu383.SP24.Api.Features.Hotels;
using Selu383.SP24.Api.Features.UserRole;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.
builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext")));

builder.Services.AddIdentity<User, Role>(options =>
  {
      options.SignIn.RequireConfirmedAccount = false;
      options.Password.RequireDigit = false;
      options.Password.RequiredLength = 6;
      options.Password.RequireNonAlphanumeric = false;
      options.Password.RequireUppercase = false;
  })
     .AddEntityFrameworkStores<DataContext>()
     .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        // Handle 401 Unauthorized
        if (context.Request.Path.StartsWithSegments("/api") && context.Response.StatusCode == StatusCodes.Status302Found)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else
        {
            // Handle 403 Forbidden or other redirects to login
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
        }

        return Task.CompletedTask;
    };
});

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
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    await SeedData(db, roleManager, userManager);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();

app
    .UseRouting()
    .UseAuthorization()
    .UseEndpoints(x =>
    {
        x.MapControllers();
    });

app.UseStaticFiles();

app.Run();

async Task SeedData(DataContext db, RoleManager<Role> roleManager, UserManager<User> userManager)
{
    await SeedRoles(roleManager);
    await SeedUsers(userManager);
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
            var role = new Role { Name = roleName, NormalizedName = roleName.ToUpper(), RoleName = roleName };
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

//see: https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0
// Hi 383 - this is added so we can test our web project automatically
public partial class Program { }
