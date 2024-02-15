using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Selu383.SP24.Api.Features.UserRole;
using System.Reflection.Emit;

namespace Selu383.SP24.Api.Data;

public class DataContext : IdentityDbContext<User, Role, int, IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
{


    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {


    }

    public DataContext()
    {
    }

    public DbSet<User> User { get; set; }
    public DbSet<Role> Role { get; set; }
    public DbSet<UserRole> UserRole { get; set; }

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


    }
}