using ArtAuction.Domain.Entities;
using ArtAuction.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Infrastructure.Persistence.Seeding;

public static class ApplicationDbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await SeedCategoriesAsync(dbContext, cancellationToken);
        await SeedAdminAsync(dbContext, cancellationToken);
        await SeedArtistAsync(dbContext, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedAdminAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        const string adminEmail = "admin@artauction.local";

        var existingAdmin = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == adminEmail, cancellationToken);

        if (existingAdmin is null)
        {
            dbContext.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Username = "seed_admin",
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin,
                IsApproved = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existingAdmin.Role = UserRole.Admin;
            existingAdmin.IsApproved = true;
            existingAdmin.IsActive = true;
            existingAdmin.UpdatedAt = DateTime.UtcNow;
        }
    }

    private static async Task SeedArtistAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        const string artistEmail = "artist@artauction.local";

        var existingArtist = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == artistEmail, cancellationToken);

        if (existingArtist is null)
        {
            dbContext.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Username = "seed_artist",
                Email = artistEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Artist@123"),
                Role = UserRole.Artist,
                IsApproved = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existingArtist.Role = UserRole.Artist;
            existingArtist.IsApproved = true;
            existingArtist.IsActive = true;
            existingArtist.UpdatedAt = DateTime.UtcNow;
        }
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        var categories = new[]
        {
            new Category
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Painting",
                Description = "Oil, acrylic, and watercolor paintings."
            },
            new Category
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Sculpture",
                Description = "Stone, metal, wood, and mixed-media sculptures."
            },
            new Category
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Digital Art",
                Description = "Illustrations, 3D renders, and digital compositions."
            }
        };

        foreach (var category in categories)
        {
            var existing = await dbContext.Categories
                .FirstOrDefaultAsync(c => c.Name == category.Name, cancellationToken);

            if (existing is null)
            {
                dbContext.Categories.Add(category);
                continue;
            }

            existing.Description = category.Description;
        }
    }
}
