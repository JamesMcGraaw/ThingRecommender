using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThingRecommender.Domain.Entities;

namespace ThingRecommender.Infrastructure.Persistence.Configurations;

public class RecommendationConfiguration : IEntityTypeConfiguration<Recommendation>
{
    public void Configure(EntityTypeBuilder<Recommendation> builder)
    {
        builder.Property(r => r.Note).HasMaxLength(1000);
        builder.Property(r => r.Score).HasAnnotation("Range", new[] { 1, 10 });
        builder.Property(r => r.ExternalRecommenderName).HasMaxLength(200);

        builder.HasOne(r => r.Recommender)
            .WithMany()
            .HasForeignKey(r => r.RecommenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Recipient)
            .WithMany()
            .HasForeignKey(r => r.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Thing)
            .WithMany()
            .HasForeignKey(r => r.ThingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.RecommenderId, r.RecipientId });
    }
}
