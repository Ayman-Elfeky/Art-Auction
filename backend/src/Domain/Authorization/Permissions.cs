namespace ArtAuction.Domain.Authorization;

public static class Permissions
{
    public const string ManageArtistAccounts = "accounts.manage.artist";
    public const string ReviewArtworks = "artworks.review";
    public const string ArtworksCreate = "artworks.create";
    public const string ArtworksUpdate = "artworks.update";
    public const string ArtworksDelete = "artworks.delete";
    public const string ArtworksExtendAuction = "artworks.extend";
    public const string BidsPlace = "bids.place";
    public const string WatchlistManage = "watchlist.manage";
    public const string UsersView = "users.view";
    public const string RolesManage = "roles.manage";
    public const string PermissionsManage = "permissions.manage";
    public const string RoleAssignmentsManage = "role.assignments.manage";
    public const string CatalogManage = "catalog.manage";
}
