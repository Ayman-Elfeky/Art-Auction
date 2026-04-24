using ArtAuction.Domain.Authorization;
using ArtAuction.Domain.Entities;
using ArtAuction.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Infrastructure.Persistence.Seeding;

public static class ApplicationDbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
    {
        // await ResetApplicationDataAsync(dbContext, cancellationToken);
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
        var adminUser = await GetOrCreateUserAsync(
            dbContext,
            "seed_admin",
            "admin@artauction.local",
            "Admin@123",
            UserRole.Admin,
            true,
            now,
            cancellationToken);
        await EnsureUserRoleAssignmentAsync(dbContext, adminUser.Id, adminRole.Id, now, cancellationToken);

        // ── Artists ──────────────────────────────────────────────────────
        var artists = new[]
        {
            await GetOrCreateUserAsync(dbContext, "Sara Ahmed",    "sara@artauction.local",   "Artist@123", UserRole.Artist, true, now, cancellationToken),
            await GetOrCreateUserAsync(dbContext, "Karim Mansour", "karim@artauction.local",  "Artist@123", UserRole.Artist, true, now, cancellationToken),
            await GetOrCreateUserAsync(dbContext, "Lena Schulz",   "lena@artauction.local",   "Artist@123", UserRole.Artist, true, now, cancellationToken),
            await GetOrCreateUserAsync(dbContext, "Amir Hassan",   "amir@artauction.local",   "Artist@123", UserRole.Artist, true, now, cancellationToken),
        };
        foreach (var a in artists)
            await EnsureUserRoleAssignmentAsync(dbContext, a.Id, artistRole.Id, now, cancellationToken);

        // ── Buyers ───────────────────────────────────────────────────────
        var buyers = new[]
        {
            await GetOrCreateUserAsync(dbContext, "Ali Omar",   "ali@artauction.local",  "Buyer@123", UserRole.Buyer, false, now, cancellationToken),
            await GetOrCreateUserAsync(dbContext, "Nour Salem", "nour@artauction.local", "Buyer@123", UserRole.Buyer, false, now, cancellationToken),
        };
        foreach (var b in buyers)
            await EnsureUserRoleAssignmentAsync(dbContext, b.Id, buyerRole.Id, now, cancellationToken);

        // ── Categories (unique Name) ─────────────────────────────────────
        var catPaintings  = await GetOrCreateCategoryAsync(dbContext, "Paintings",   "Oil, acrylic, watercolour and mixed-media paintings", cancellationToken);
        var catSculptures = await GetOrCreateCategoryAsync(dbContext, "Sculptures",  "Three-dimensional works in stone, metal, clay and wood", cancellationToken);
        var catDigital    = await GetOrCreateCategoryAsync(dbContext, "Digital Art", "Computer-generated and NFT-style digital artworks", cancellationToken);
        var catPhotos     = await GetOrCreateCategoryAsync(dbContext, "Photography", "Fine-art photography and photo prints", cancellationToken);
        var catDrawings   = await GetOrCreateCategoryAsync(dbContext, "Drawings",    "Pencil, ink, charcoal and pastel drawings", cancellationToken);

        // ── Artworks (Active auctions — end 72 h from now) ───────────────
        var end72  = now.AddHours(72);
        var end48  = now.AddHours(48);
        var end24  = now.AddHours(24);
        var end120 = now.AddHours(120);
        var start  = now.AddMinutes(-5);

        var artworkSeeds = new (Guid ArtistId, Guid CategoryId, string Title, string Description, decimal Initial, decimal? BuyNow, DateTime Start, DateTime End, string ImageUrl, string[] Tags)[]
        {
            (artists[0].Id, catPaintings.Id,  "Desert Silence",       "A vast golden desert at dusk, oil on canvas.",             1200, 3500,  start, end72,  "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=800", new[]{"oil","landscape","desert","abstract"}),
            (artists[0].Id, catPaintings.Id,  "Blue Reverie",         "Abstract swirls of azure and cobalt, acrylic.",            800,  null,  start, end48,  "https://images.unsplash.com/photo-1541701494587-cb58502866ab?w=800", new[]{"abstract","acrylic","blue"}),
            (artists[1].Id, catSculptures.Id, "Eternal Form",         "Marble figure inspired by classical antiquity.",            2500, 8000,  start, end120, "https://images.unsplash.com/photo-1561839561-b13bcfe95249?w=800", new[]{"marble","classical","sculpture"}),
            (artists[1].Id, catDigital.Id,    "Neon Genesis",         "Cyberpunk cityscape rendered in Blender.",                 600,  2000,  start, end24,  "https://images.unsplash.com/photo-1518770660439-4636190af475?w=800", new[]{"digital","neon","cyberpunk","3d"}),
            (artists[2].Id, catPhotos.Id,     "Foggy Mountains",      "Long-exposure mountain photography at dawn.",              900,  null,  start, end72,  "https://images.unsplash.com/photo-1506905925346-21bda4d32df4?w=800", new[]{"photography","landscape","nature"}),
            (artists[2].Id, catPaintings.Id,  "Portrait in Gold",     "Hyper-realistic portrait with gold-leaf accents.",         1800, 5000,  start, end48,  "https://images.unsplash.com/photo-1500462918059-b1a0cb512f1d?w=800", new[]{"portrait","oil","gold","realism"}),
            (artists[3].Id, catDrawings.Id,   "Ink Dreams",           "Intricate ink illustration of a dreamlike forest.",        450,  1200,  start, end120, "https://images.unsplash.com/photo-1513364776144-60967b0f800f?w=800", new[]{"ink","drawing","forest","illustration"}),
            (artists[3].Id, catDigital.Id,    "Fractal Universe",     "Generative art — infinite fractal zoom.",                  350,  null,  start, end24,  "https://images.unsplash.com/photo-1534796636912-3b95b3ab5986?w=800", new[]{"digital","generative","fractal","abstract"}),
            (artists[0].Id, catSculptures.Id, "Bronze Horizon",       "Abstract bronze cast exploring the concept of horizons.",  3200, 9000,  start, end72,  "https://images.unsplash.com/photo-1565814329452-e1efa11c5b89?w=800", new[]{"bronze","abstract","sculpture"}),
            (artists[1].Id, catPaintings.Id,  "Autumn Harvest",       "Warm autumnal landscape, palette knife technique.",       950,  2800,  start, end48,  "https://images.unsplash.com/photo-1508739773434-c26b3d09e071?w=800", new[]{"oil","landscape","autumn","palette knife"}),
        };

        foreach (var seed in artworkSeeds)
        {
            await EnsureArtworkWithTagsAsync(
                dbContext,
                seed.ArtistId,
                seed.CategoryId,
                seed.Title,
                seed.Description,
                seed.Initial,
                seed.BuyNow,
                seed.Start,
                seed.End,
                seed.ImageUrl,
                seed.Tags,
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<User> GetOrCreateUserAsync(
        ApplicationDbContext dbContext,
        string username,
        string email,
        string password,
        UserRole role,
        bool isApproved,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (existing is not null)
            return existing;

        var user = CreateUser(username, email, password, role, isApproved, now);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    private static async Task EnsureUserRoleAssignmentAsync(
        ApplicationDbContext dbContext,
        Guid userId,
        Guid roleId,
        DateTime assignedAt,
        CancellationToken cancellationToken)
    {
        // Schema allows at most one role row per user (unique index on UserId).
        if (await dbContext.UserRoleAssignments.AnyAsync(ua => ua.UserId == userId, cancellationToken))
            return;

        dbContext.UserRoleAssignments.Add(new UserRoleAssignment
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = assignedAt
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<Category> GetOrCreateCategoryAsync(
        ApplicationDbContext dbContext,
        string name,
        string description,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.Categories.FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
        if (existing is not null)
            return existing;

        var category = new Category { Id = Guid.NewGuid(), Name = name, Description = description };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);
        return category;
    }

    private static async Task EnsureArtworkWithTagsAsync(
        ApplicationDbContext dbContext,
        Guid artistId,
        Guid categoryId,
        string title,
        string description,
        decimal initialPrice,
        decimal? buyNowPrice,
        DateTime startTime,
        DateTime endTime,
        string imageUrl,
        string[] tags,
        CancellationToken cancellationToken)
    {
        var artwork = await dbContext.Artworks
            .FirstOrDefaultAsync(a => a.ArtistId == artistId && a.Title == title, cancellationToken);

        if (artwork is null)
        {
            artwork = MakeArtwork(artistId, categoryId, title, description, initialPrice, buyNowPrice, startTime, endTime, imageUrl);
            dbContext.Artworks.Add(artwork);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        foreach (var tag in tags)
        {
            var hasTag = await dbContext.ArtworkTags
                .AnyAsync(at => at.ArtworkId == artwork.Id && at.Tag == tag, cancellationToken);
            if (hasTag)
                continue;

            dbContext.ArtworkTags.Add(new ArtworkTag
            {
                Id = Guid.NewGuid(),
                ArtworkId = artwork.Id,
                Tag = tag
            });
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
