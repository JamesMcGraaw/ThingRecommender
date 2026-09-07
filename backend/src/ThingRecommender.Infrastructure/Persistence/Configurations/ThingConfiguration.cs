using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThingRecommender.Domain.Entities;

namespace ThingRecommender.Infrastructure.Persistence.Configurations;

public class ThingConfiguration : IEntityTypeConfiguration<Thing>
{
    public void Configure(EntityTypeBuilder<Thing> builder)
    {
        builder.Property(t => t.Title).HasMaxLength(300).IsRequired();
        builder.Property(t => t.MediaType).HasConversion<string>().HasMaxLength(20);
    }
}
