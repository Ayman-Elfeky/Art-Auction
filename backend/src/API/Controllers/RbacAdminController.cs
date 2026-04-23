using ArtAuction.Domain.Authorization;
using ArtAuction.Domain.Entities;
using ArtAuction.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/rbac")]
public sealed class RbacAdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RbacAdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("roles")]
    [Authorize(Policy = RbacPolicies.ListRoles)]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _context.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.Name)
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Description,
                r.IsSystem,
                permissions = r.RolePermissions.Select(rp => rp.Permission.Name).OrderBy(x => x).ToList()
            })
            .ToListAsync();

        return Ok(roles);
    }

    [HttpPost("roles")]
    [Authorize(Policy = Permissions.RolesManage)]
    public async Task<IActionResult> CreateRole([FromBody] RoleUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "Role name is required." });
        }

        var exists = await _context.Roles.AnyAsync(r => r.Name == request.Name);
        if (exists)
        {
            return Conflict(new { error = "Role already exists." });
        }

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            IsSystem = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Roles.Add(role);
        await SyncRolePermissionsAsync(role.Id, request.Permissions ?? []);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Role created.", roleId = role.Id });
    }

    [HttpPut("roles/{roleId:guid}")]
    [Authorize(Policy = Permissions.RolesManage)]
    public async Task<IActionResult> UpdateRole(Guid roleId, [FromBody] RoleUpsertRequest request)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
        if (role is null)
        {
            return NotFound(new { error = "Role not found." });
        }

        role.Description = request.Description?.Trim() ?? role.Description;
        if (!role.IsSystem && !string.IsNullOrWhiteSpace(request.Name))
        {
            role.Name = request.Name.Trim();
        }
        role.UpdatedAt = DateTime.UtcNow;

        await SyncRolePermissionsAsync(role.Id, request.Permissions ?? []);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Role updated." });
    }

    [HttpDelete("roles/{roleId:guid}")]
    [Authorize(Policy = Permissions.RolesManage)]
    public async Task<IActionResult> DeleteRole(Guid roleId)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
        if (role is null)
        {
            return NotFound(new { error = "Role not found." });
        }

        if (role.IsSystem)
        {
            return BadRequest(new { error = "System roles cannot be deleted." });
        }

        var assigned = await _context.UserRoleAssignments.AnyAsync(ua => ua.RoleId == roleId);
        if (assigned)
        {
            return BadRequest(new { error = "Role is assigned to users. Reassign first." });
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Role deleted." });
    }

    [HttpGet("permissions")]
    [Authorize(Policy = RbacPolicies.ListPermissions)]
    public async Task<IActionResult> GetPermissions()
    {
        var permissions = await _context.Permissions
            .OrderBy(p => p.Name)
            .Select(p => new { p.Id, p.Name, p.Description })
            .ToListAsync();
        return Ok(permissions);
    }

    [HttpPost("permissions")]
    [Authorize(Policy = Permissions.PermissionsManage)]
    public async Task<IActionResult> CreatePermission([FromBody] PermissionUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "Permission name is required." });
        }

        var exists = await _context.Permissions.AnyAsync(p => p.Name == request.Name);
        if (exists)
        {
            return Conflict(new { error = "Permission already exists." });
        }

        _context.Permissions.Add(new Permission
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        return Ok(new { message = "Permission created." });
    }

    [HttpPut("permissions/{permissionId:guid}")]
    [Authorize(Policy = Permissions.PermissionsManage)]
    public async Task<IActionResult> UpdatePermission(Guid permissionId, [FromBody] PermissionUpsertRequest request)
    {
        var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == permissionId);
        if (permission is null)
        {
            return NotFound(new { error = "Permission not found." });
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            permission.Name = request.Name.Trim();
        }
        permission.Description = request.Description?.Trim() ?? permission.Description;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Permission updated." });
    }

    [HttpDelete("permissions/{permissionId:guid}")]
    [Authorize(Policy = Permissions.PermissionsManage)]
    public async Task<IActionResult> DeletePermission(Guid permissionId)
    {
        var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == permissionId);
        if (permission is null)
        {
            return NotFound(new { error = "Permission not found." });
        }

        var inUse = await _context.RolePermissions.AnyAsync(rp => rp.PermissionId == permissionId);
        if (inUse)
        {
            return BadRequest(new { error = "Permission is assigned to roles. Remove assignments first." });
        }

        _context.Permissions.Remove(permission);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Permission deleted." });
    }

    [HttpGet("users")]
    [Authorize(Policy = Permissions.UsersView)]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .Include(u => u.RoleAssignments)
            .ThenInclude(ua => ua.Role)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.IsApproved,
                u.IsActive,
                role = u.RoleAssignments.Select(ua => ua.Role.Name).FirstOrDefault() ?? u.Role.ToString(),
                u.CreatedAt
            })
            .ToListAsync();
        return Ok(users);
    }

    [HttpPut("users/{userId:guid}/role")]
    [Authorize(Policy = Permissions.RoleAssignmentsManage)]
    public async Task<IActionResult> AssignRoleToUser(Guid userId, [FromBody] AssignRoleRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            return NotFound(new { error = "User not found." });
        }

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId);
        if (role is null)
        {
            return NotFound(new { error = "Role not found." });
        }

        var existingAssignments = await _context.UserRoleAssignments
            .Where(ua => ua.UserId == userId)
            .ToListAsync();
        if (existingAssignments.Count > 0)
        {
            _context.UserRoleAssignments.RemoveRange(existingAssignments);
        }

        _context.UserRoleAssignments.Add(new UserRoleAssignment
        {
            UserId = userId,
            RoleId = role.Id,
            AssignedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return Ok(new { message = "Role assigned to user." });
    }

    private async Task SyncRolePermissionsAsync(Guid roleId, IEnumerable<string> permissionNames)
    {
        var normalized = permissionNames
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var rolePermissions = await _context.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync();
        _context.RolePermissions.RemoveRange(rolePermissions);

        if (normalized.Count == 0)
        {
            return;
        }

        var permissions = await _context.Permissions
            .Where(p => normalized.Contains(p.Name))
            .ToListAsync();

        foreach (var permission in permissions)
        {
            _context.RolePermissions.Add(new RolePermission
            {
                RoleId = roleId,
                PermissionId = permission.Id
            });
        }
    }
}

public sealed class RoleUpsertRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<string>? Permissions { get; set; }
}

public sealed class PermissionUpsertRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public sealed class AssignRoleRequest
{
    public Guid RoleId { get; set; }
}
