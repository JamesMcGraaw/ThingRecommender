using Microsoft.EntityFrameworkCore;
using ThingRecommender.Application.Abstractions;
using ThingRecommender.Domain.Entities;

namespace ThingRecommender.Application.UnitTests;

/// <summary>Minimal in-memory stand-in for the real EF Core DbContext, so Application-layer
/// services can be tested without depending on the Infrastructure project.</summary>
public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Thing> Things => Set<Thing>();
    public DbSet<Recommendation> Recommendations => Set<Recommendation>();

    public static TestDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }
}
