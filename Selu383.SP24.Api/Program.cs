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
    var users = db.Set<User>();
    if (!await users.AnyAsync())
    {
        await UserManager.CreateAsync(new User { UserName = "galkadi" });
    }

    var userManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    var roles = db.Set<User>();
    if (!await users.AnyAsync())
    {
        await RoleManager.CreateAsync(new User { UserName = "galkadi" });
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

//see: https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0
// Hi 383 - this is added so we can test our web project automatically
public partial class Program { }
