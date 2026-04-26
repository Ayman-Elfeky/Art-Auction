using ArtAuction.Domain.Enums;

namespace ArtAuction.Domain.Entities;

public class User
{
    public Guid Id {get; set;}
    public string Username {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string PasswordHash {get; set;} = string.Empty;
    public UserRole Role {get; set;}
    public bool IsApproved {get; set;}
    public bool IsActive {get; set;} = true;
    public string? ProfilePictureUrl {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime UpdatedAt {get; set;} = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Artwork> Artworks {get; set;} = new List<Artwork>();
    public ICollection<Bid> Bids {get; set;} = new List<Bid>();
    public ICollection<Watchlist> Watchlist { get; set; } = new List<Watchlist>();
    public ICollection<Notification> Notifications {get; set;} = new List<Notification>();
    public ICollection<UserRoleAssignment> RoleAssignments { get; set; } = new List<UserRoleAssignment>();
}