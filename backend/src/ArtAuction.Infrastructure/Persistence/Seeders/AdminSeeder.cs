using ArtAuction.Domain.Entities;
using ArtAuction.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Infrastructure.Persistence.Seeders;

public static class AdminSeeder
{
    public static async Task SeedAdminAsync(ApplicationDbContext context)
    {
        var adminExists = await context.Users.AnyAsync(u => u.Email == "admin@example.com");
        
        if (adminExists)
            return;

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            Email = "admin@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = UserRole.Admin,
            IsApproved = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }
}
