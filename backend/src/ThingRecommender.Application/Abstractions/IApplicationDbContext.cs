using Microsoft.EntityFrameworkCore;
using ThingRecommender.Domain.Entities;

namespace ThingRecommender.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Thing> Things { get; }
    DbSet<Recommendation> Recommendations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
