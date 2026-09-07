using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThingRecommender.Domain.Entities;
using ThingRecommender.Domain.Seed;

namespace ThingRecommender.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(320).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();

        // Seeded so local dev has data to look at; a real sign-in with a matching email links to these
        // rather than creating duplicates (see AuthService).
        builder.HasData(
            new User { Id = SeedUserIds.Alice, DisplayName = "Alice", Email = "alice@example.com" },
            new User { Id = SeedUserIds.Bob, DisplayName = "Bob", Email = "bob@example.com" });
    }
}
