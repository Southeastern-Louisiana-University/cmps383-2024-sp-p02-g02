using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Selu383.SP24.Api.Data;
using Selu383.SP24.Api.Features.Hotels;
using Selu383.SP24.Api.Features.UserRole;


var builder = WebApplication.CreateBuilder(args);

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

//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.Events.OnRedirectToLogin = context =>
//    {
//        // Handle 401 Unauthorized
//        if (context.Request.Path.StartsWithSegments("/api") && context.Response.StatusCode == StatusCodes.Status302Found)
//        {
//            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
//        }
//        else
//        {
//            // Handle 403 Forbidden or other redirects to login
//            context.Response.StatusCode = StatusCodes.Status403Forbidden;
//        }

//        return Task.CompletedTask;
//    };
//});

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events = new CookieAuthenticationEvents
    {

        OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api") && context.Response.StatusCode == StatusCodes.Status302Found)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            }

            return Task.CompletedTask;
        }
    };
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;

});


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




async Task SeedData(DataContext db, RoleManager<Role> roleManager, UserManager<User> userManager)
{
    await SeedRoles(roleManager);
    await SeedUsers(userManager);
    await db.SaveChangesAsync();
}

async Task SeedRoles(RoleManager<Role> roleManager)
{
    string[] roleNames = ["Admin", "User"];

    foreach (var roleName in roleNames)
    {
        var roleExists = await roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            // Create the role using RoleManager
            var role = new Role { Name = roleName, NormalizedName = roleName.ToUpper()};
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
    }
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();

app 
    .UseRouting();
app.UseEndpoints(x =>
{
    x.MapControllers();

});

app.UseStaticFiles();

 if (app.Environment.IsDevelopment())
{
    app.UseSpa(x =>
    {
        x.UseProxyToSpaDevelopmentServer("http://localhost:5173");
    });
}
else
{
    app.MapFallbackToFile("index.html");
}

app.UseAuthorization();

app.MapControllers();

app.Run();



//see: https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0
// Hi 383 - this is added so we can test our web project automatically
public partial class Program { }
