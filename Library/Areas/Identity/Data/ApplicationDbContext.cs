using Library.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;
using Library.Models;

namespace Library.Areas.Identity.Data;

public class ApplicationDbContext : IdentityDbContext<LibraryUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);

        builder.Entity<LibraryUser>().Ignore(c => c.AccessFailedCount)
                                           .Ignore(c => c.Email)
                                           .Ignore(c => c.EmailConfirmed)
                                           .Ignore(c => c.LockoutEnabled)
                                           .Ignore(c => c.LockoutEnd)
                                           .Ignore(c => c.NormalizedEmail)
                                           .Ignore(c => c.PhoneNumber)
                                           .Ignore(c => c.PhoneNumberConfirmed)
                                           .Ignore(c => c.TwoFactorEnabled);

       // builder.Entity<IdentityUser>().ToTable("Users");//to change the name of table.

        builder.ApplyConfiguration(new LibraryUserEntityConfiguration());
    }

   // public DbSet<Library.Models.ProjectRole> ProjectRole { get; set; }
}

public class LibraryUserEntityConfiguration : IEntityTypeConfiguration<LibraryUser>
{
    void IEntityTypeConfiguration<LibraryUser>.Configure(EntityTypeBuilder<LibraryUser> builder)
    {
        builder.Property(u => u.FirstName).HasMaxLength(128);
        builder.Property(u => u.LastName).HasMaxLength(128);
    }
}