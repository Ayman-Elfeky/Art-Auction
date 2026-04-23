using ArtAuction.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Api.Authorization;

public static class PolicyRegistration
{
    public static void AddPermissionPolicy(AuthorizationOptions options, string policyName, params string[] anyPermissionClaim)
    {
        var names = anyPermissionClaim;
        options.AddPolicy(
            policyName,
            p => p.RequireAssertion(ctx =>
            {
                var set = ctx.User.FindAll("permission").Select(c => c.Value).ToHashSet();
                return names.Any(n => set.Contains(n));
            }));
    }

    public static void Register(AuthorizationOptions options)
    {
        AddPermissionPolicy(
            options,
            Permissions.ManageArtistAccounts,
            Permissions.ManageArtistAccounts,
            "approve.artist");
        AddPermissionPolicy(
            options,
            Permissions.ReviewArtworks,
            Permissions.ReviewArtworks,
            "approve.artwork");
        AddPermissionPolicy(
            options,
            Permissions.CatalogManage,
            Permissions.CatalogManage);

        AddPermissionPolicy(
            options,
            Permissions.ArtworksCreate,
            Permissions.ArtworksCreate);
        AddPermissionPolicy(
            options,
            Permissions.ArtworksUpdate,
            Permissions.ArtworksUpdate);
        AddPermissionPolicy(
            options,
            Permissions.ArtworksDelete,
            Permissions.ArtworksDelete);
        AddPermissionPolicy(
            options,
            Permissions.ArtworksExtendAuction,
            Permissions.ArtworksExtendAuction);
        AddPermissionPolicy(
            options,
            Permissions.BidsPlace,
            Permissions.BidsPlace);
        AddPermissionPolicy(
            options,
            Permissions.WatchlistManage,
            Permissions.WatchlistManage);
        AddPermissionPolicy(
            options,
            Permissions.UsersView,
            Permissions.UsersView,
            "view.users",
            Permissions.RoleAssignmentsManage);
        AddPermissionPolicy(
            options,
            Permissions.RolesManage,
            Permissions.RolesManage);
        AddPermissionPolicy(
            options,
            Permissions.PermissionsManage,
            Permissions.PermissionsManage);
        AddPermissionPolicy(
            options,
            Permissions.RoleAssignmentsManage,
            Permissions.RoleAssignmentsManage);

        AddPermissionPolicy(
            options,
            RbacPolicies.ListRoles,
            Permissions.RolesManage,
            Permissions.RoleAssignmentsManage,
            Permissions.PermissionsManage);
        AddPermissionPolicy(
            options,
            RbacPolicies.ListPermissions,
            Permissions.PermissionsManage,
            Permissions.RolesManage);
    }
}
