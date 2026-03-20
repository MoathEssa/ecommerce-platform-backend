using ECommerceCenter.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceCenter.Infrastructure.Data.Configurations.Auth;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> entity)
    {
        // UserId is both PK and FK — no identity column
        entity.HasKey(p => p.UserId);
        entity.Property(p => p.UserId).ValueGeneratedNever();

        entity.Property(p => p.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(p => p.LastName)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(p => p.Phone).HasMaxLength(50);

        entity.Property(p => p.AvatarUrl).HasMaxLength(500);

        entity.Property(p => p.Gender).HasMaxLength(32);

        entity.Property(p => p.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Ignore computed property — derived in app, not stored
        entity.Ignore(p => p.FullName);

        // 1-to-1 with ApplicationUser
        entity.HasOne(p => p.User)
            .WithOne(u => u.Person)
            .HasForeignKey<Person>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
