using ArtAuction.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VeldGenerated.Models;
using VeldGenerated.Services;

namespace Api.Services;

public class CategoriesService : ICategoriesService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CategoriesService(IApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<Category>> ListCategories()
    {
        return await _dbContext.Categories
            .OrderBy(c => c.Name)
            .Select(c => new Category(c.Id, c.Name, c.Description))
            .ToListAsync();
    }

    public async Task<Category> CreateCategory(CreateCategoryInput input)
    {
        // EnsureAdmin();

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new InvalidOperationException("Category name is required.");
        }

        var normalizedName = input.Name.Trim();
        var normalizedLookup = normalizedName.ToLowerInvariant();
        var normalizedDescription = input.Description?.Trim();
        var existing = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == normalizedLookup);

        if (existing is not null)
        {
            if (!string.IsNullOrWhiteSpace(normalizedDescription) && existing.Description != normalizedDescription)
            {
                existing.Description = normalizedDescription;
                await _dbContext.SaveChangesAsync();
            }

            return new Category(existing.Id, existing.Name, existing.Description);
        }

        var category = new ArtAuction.Domain.Entities.Category
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Description = normalizedDescription ?? string.Empty
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        return new Category(category.Id, category.Name, category.Description);
    }

    private void EnsureAdmin()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No HTTP context found.");
        var authHeader = httpContext.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Missing bearer token.");
        }

        var token = authHeader["Bearer ".Length..].Trim();
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;

        if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only admin users can create categories.");
        }
    }
}
