using ArtAuction.Domain.Authorization;
using ArtAuction.Domain.Entities;
using ArtAuction.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Infrastructure.Persistence.Seeding;

public static class ApplicationDbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await ResetApplicationDataAsync(dbContext, cancellationToken);
        await SeedRolesAndPermissionsAsync(dbContext, cancellationToken);
        await SeedDemoDataAsync(dbContext, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Task ResetApplicationDataAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        return dbContext.Database.ExecuteSqlRawAsync(
            """
            TRUNCATE TABLE
                "UserRoleAssignments",
                "RolePermissions",
                "Permissions",
                "Roles",
                "Watchlists",
                "Bids",
                "ArtworkTags",
                "Notifications",
                "Artworks",
                "Categories",
                "Users"
            RESTART IDENTITY CASCADE;
            """,
            cancellationToken);
    }

    private static async Task SeedDemoDataAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        var adminRole  = await dbContext.Roles.FirstAsync(r => r.Name == "Admin",  cancellationToken);
        var artistRole = await dbContext.Roles.FirstAsync(r => r.Name == "Artist", cancellationToken);
        var buyerRole  = await dbContext.Roles.FirstAsync(r => r.Name == "Buyer",  cancellationToken);
        var now = DateTime.UtcNow;

        // ── Admin ────────────────────────────────────────────────────────
        var adminUser = CreateUser("seed_admin", "admin@artauction.local", "Admin@123", UserRole.Admin, true, now);
        dbContext.Users.Add(adminUser);
        await dbContext.SaveChangesAsync(cancellationToken);
        dbContext.UserRoleAssignments.Add(new UserRoleAssignment { UserId = adminUser.Id, RoleId = adminRole.Id, AssignedAt = now });

        // ── Artists ──────────────────────────────────────────────────────
        var artists = new[]
        {
            CreateUser("Sara Ahmed",    "sara@artauction.local",   "Artist@123", UserRole.Artist, true, now),
            CreateUser("Karim Mansour", "karim@artauction.local",  "Artist@123", UserRole.Artist, true, now),
            CreateUser("Lena Schulz",   "lena@artauction.local",   "Artist@123", UserRole.Artist, true, now),
            CreateUser("Amir Hassan",   "amir@artauction.local",   "Artist@123", UserRole.Artist, true, now),
        };
        dbContext.Users.AddRange(artists);
        await dbContext.SaveChangesAsync(cancellationToken);
        foreach (var a in artists)
            dbContext.UserRoleAssignments.Add(new UserRoleAssignment { UserId = a.Id, RoleId = artistRole.Id, AssignedAt = now });

        // ── Buyers ───────────────────────────────────────────────────────
        var buyers = new[]
        {
            CreateUser("Ali Omar",   "ali@artauction.local",  "Buyer@123", UserRole.Buyer, false, now),
            CreateUser("Nour Salem", "nour@artauction.local", "Buyer@123", UserRole.Buyer, false, now),
        };
        dbContext.Users.AddRange(buyers);
        await dbContext.SaveChangesAsync(cancellationToken);
        foreach (var b in buyers)
            dbContext.UserRoleAssignments.Add(new UserRoleAssignment { UserId = b.Id, RoleId = buyerRole.Id, AssignedAt = now });

        await dbContext.SaveChangesAsync(cancellationToken);

        // ── Categories ───────────────────────────────────────────────────
        var catPaintings  = MakeCategory("Paintings",   "Oil, acrylic, watercolour and mixed-media paintings");
        var catSculptures = MakeCategory("Sculptures",  "Three-dimensional works in stone, metal, clay and wood");
        var catDigital    = MakeCategory("Digital Art", "Computer-generated and NFT-style digital artworks");
        var catPhotos     = MakeCategory("Photography", "Fine-art photography and photo prints");
        var catDrawings   = MakeCategory("Drawings",    "Pencil, ink, charcoal and pastel drawings");
        var categories    = new[] { catPaintings, catSculptures, catDigital, catPhotos, catDrawings };
        dbContext.Categories.AddRange(categories);
        await dbContext.SaveChangesAsync(cancellationToken);

        // ── Artworks (Active auctions — end 72 h from now) ───────────────
        var end72  = now.AddHours(72);
        var end48  = now.AddHours(48);
        var end24  = now.AddHours(24);
        var end120 = now.AddHours(120);
        var start  = now.AddMinutes(-5);

        var artworks = new List<(Artwork artwork, string[] tags)>
        {
            (MakeArtwork(artists[0].Id, catPaintings.Id,  "Desert Silence",       "A vast golden desert at dusk, oil on canvas.",             1200, 3500,  start, end72,  "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=800"), new[]{"oil","landscape","desert","abstract"}),
            (MakeArtwork(artists[0].Id, catPaintings.Id,  "Blue Reverie",         "Abstract swirls of azure and cobalt, acrylic.",            800,  null,  start, end48,  "https://images.unsplash.com/photo-1541701494587-cb58502866ab?w=800"), new[]{"abstract","acrylic","blue"}),
            (MakeArtwork(artists[1].Id, catSculptures.Id, "Eternal Form",         "Marble figure inspired by classical antiquity.",            2500, 8000,  start, end120, "https://images.unsplash.com/photo-1561839561-b13bcfe95249?w=800"), new[]{"marble","classical","sculpture"}),
            (MakeArtwork(artists[1].Id, catDigital.Id,    "Neon Genesis",         "Cyberpunk cityscape rendered in Blender.",                 600,  2000,  start, end24,  "https://images.unsplash.com/photo-1518770660439-4636190af475?w=800"), new[]{"digital","neon","cyberpunk","3d"}),
            (MakeArtwork(artists[2].Id, catPhotos.Id,     "Foggy Mountains",      "Long-exposure mountain photography at dawn.",              900,  null,  start, end72,  "https://images.unsplash.com/photo-1506905925346-21bda4d32df4?w=800"), new[]{"photography","landscape","nature"}),
            (MakeArtwork(artists[2].Id, catPaintings.Id,  "Portrait in Gold",     "Hyper-realistic portrait with gold-leaf accents.",         1800, 5000,  start, end48,  "https://images.unsplash.com/photo-1500462918059-b1a0cb512f1d?w=800"), new[]{"portrait","oil","gold","realism"}),
            (MakeArtwork(artists[3].Id, catDrawings.Id,   "Ink Dreams",           "Intricate ink illustration of a dreamlike forest.",        450,  1200,  start, end120, "https://images.unsplash.com/photo-1513364776144-60967b0f800f?w=800"), new[]{"ink","drawing","forest","illustration"}),
            (MakeArtwork(artists[3].Id, catDigital.Id,    "Fractal Universe",     "Generative art — infinite fractal zoom.",                  350,  null,  start, end24,  "https://images.unsplash.com/photo-1534796636912-3b95b3ab5986?w=800"), new[]{"digital","generative","fractal","abstract"}),
            (MakeArtwork(artists[0].Id, catSculptures.Id, "Bronze Horizon",       "Abstract bronze cast exploring the concept of horizons.",  3200, 9000,  start, end72,  "https://images.unsplash.com/photo-1565814329452-e1efa11c5b89?w=800"), new[]{"bronze","abstract","sculpture"}),
            (MakeArtwork(artists[1].Id, catPaintings.Id,  "Autumn Harvest",       "Warm autumnal landscape, palette knife technique.",       950,  2800,  start, end48,  "https://images.unsplash.com/photo-1508739773434-c26b3d09e071?w=800"), new[]{"oil","landscape","autumn","palette knife"}),
        };

        foreach (var (artwork, tags) in artworks)
        {
            dbContext.Artworks.Add(artwork);
            await dbContext.SaveChangesAsync(cancellationToken);
            foreach (var tag in tags)
                dbContext.ArtworkTags.Add(new ArtworkTag { Id = Guid.NewGuid(), ArtworkId = artwork.Id, Tag = tag });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // ── Helpers ──────────────────────────────────────────────────────────
    private static User CreateUser(string username, string email, string password, UserRole role, bool isApproved, DateTime now) =>
        new()
        {
            Id           = Guid.NewGuid(),
            Username     = username,
            Email        = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role         = role,
            IsApproved   = isApproved,
            IsActive     = true,
            CreatedAt    = now,
            UpdatedAt    = now,
        };

    private static Category MakeCategory(string name, string description) =>
        new() { Id = Guid.NewGuid(), Name = name, Description = description };

    private static Artwork MakeArtwork(
        Guid artistId, Guid categoryId, string title, string description,
        decimal initialPrice, decimal? buyNowPrice,
        DateTime startTime, DateTime endTime, string imageUrl) =>
        new()
        {
            Id               = Guid.NewGuid(),
            ArtistId         = artistId,
            CategoryId       = categoryId,
            Title            = title,
            Description      = description,
            InitialPrice     = initialPrice,
            CurrentBid       = initialPrice,
            BuyNowPrice      = buyNowPrice,
            AuctionStartTime = startTime,
            AuctionEndTime   = endTime,
            Status           = ArtworkStatus.Active,
            ImageUrl         = imageUrl,
            CreatedAt        = DateTime.UtcNow,
            UpdatedAt        = DateTime.UtcNow,
        };

    private static async Task SeedRolesAndPermissionsAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        var defaultPermissions = new[]
        {
            Permissions.ManageArtistAccounts,
            Permissions.ReviewArtworks,
            Permissions.ArtworksCreate,
            Permissions.ArtworksUpdate,
            Permissions.ArtworksDelete,
            Permissions.ArtworksExtendAuction,
            Permissions.BidsPlace,
            Permissions.WatchlistManage,
            Permissions.UsersView,
            Permissions.CatalogManage,
            Permissions.RolesManage,
            Permissions.PermissionsManage,
            Permissions.RoleAssignmentsManage
        };

        foreach (var permissionName in defaultPermissions)
        {
            var existing = await dbContext.Permissions.FirstOrDefaultAsync(p => p.Name == permissionName, cancellationToken);
            if (existing is null)
                dbContext.Permissions.Add(new Permission { Id = Guid.NewGuid(), Name = permissionName, Description = permissionName, CreatedAt = DateTime.UtcNow });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await EnsureRoleAsync(dbContext, "Admin", true, defaultPermissions, cancellationToken);
        await EnsureRoleAsync(dbContext, "Artist", true, new[]
        {
            Permissions.ArtworksCreate,
            Permissions.ArtworksUpdate,
            Permissions.ArtworksDelete,
            Permissions.ArtworksExtendAuction
        }, cancellationToken);
        await EnsureRoleAsync(dbContext, "Buyer", true, new[]
        {
            Permissions.BidsPlace,
            Permissions.WatchlistManage
        }, cancellationToken);
    }

    private static async Task EnsureRoleAsync(
        ApplicationDbContext dbContext,
        string roleName,
        bool isSystem,
        IEnumerable<string> permissions,
        CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);

        if (role is null)
        {
            role = new Role
            {
                Id          = Guid.NewGuid(),
                Name        = roleName,
                Description = $"{roleName} role",
                IsSystem    = isSystem,
                CreatedAt   = DateTime.UtcNow,
                UpdatedAt   = DateTime.UtcNow,
            };
            dbContext.Roles.Add(role);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        foreach (var permissionName in permissions)
        {
            var permission = await dbContext.Permissions.FirstAsync(p => p.Name == permissionName, cancellationToken);
            var has = await dbContext.RolePermissions.AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id, cancellationToken);
            if (!has)
                dbContext.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
        }
    }
}
