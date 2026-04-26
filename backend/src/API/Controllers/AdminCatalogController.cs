using ArtAuction.Domain.Authorization;
using ArtAuction.Domain.Entities;
using ArtAuction.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/admin/catalog")]
[Authorize(Policy = Permissions.CatalogManage)]
public sealed class AdminCatalogController : ControllerBase
{
	private readonly ApplicationDbContext _context;

	public AdminCatalogController(ApplicationDbContext context)
	{
		_context = context;
	}

	[HttpGet("categories")]
	public async Task<IActionResult> GetCategories()
	{
		var categories = await _context.Categories
			.OrderBy(c => c.Name)
			.Select(c => new
			{
				id = c.Id,
				name = c.Name,
				description = c.Description
			})
			.ToListAsync();

		return Ok(categories);
	}

	[HttpPost("categories")]
	public async Task<IActionResult> CreateCategory([FromBody] CategoryUpsertRequest request)
	{
		var name = request.Name?.Trim();
		var description = request.Description?.Trim() ?? string.Empty;

		if (string.IsNullOrWhiteSpace(name))
		{
			return BadRequest(new { message = "Category name is required." });
		}

		var exists = await _context.Categories
			.AnyAsync(c => c.Name.ToLower() == name.ToLower());

		if (exists)
		{
			return Conflict(new { message = "Category already exists." });
		}

		var category = new Category
		{
			Id = Guid.NewGuid(),
			Name = name,
			Description = description
		};

		_context.Categories.Add(category);
		await _context.SaveChangesAsync();

		return Ok(new
		{
			message = "Category created.",
			id = category.Id,
			name = category.Name,
			description = category.Description
		});
	}

	[HttpPut("categories/{id:guid}")]
	public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CategoryUpsertRequest request)
	{
		var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
		if (category is null)
		{
			return NotFound(new { message = "Category not found." });
		}

		var name = request.Name?.Trim();
		var description = request.Description?.Trim() ?? string.Empty;

		if (string.IsNullOrWhiteSpace(name))
		{
			return BadRequest(new { message = "Category name is required." });
		}

		var duplicate = await _context.Categories
			.AnyAsync(c => c.Id != id && c.Name.ToLower() == name.ToLower());

		if (duplicate)
		{
			return Conflict(new { message = "Category already exists." });
		}

		category.Name = name;
		category.Description = description;

		await _context.SaveChangesAsync();
		return Ok(new { message = "Category updated." });
	}

	[HttpDelete("categories/{id:guid}")]
	public async Task<IActionResult> DeleteCategory(Guid id)
	{
		var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
		if (category is null)
		{
			return NotFound(new { message = "Category not found." });
		}

		var hasArtworks = await _context.Artworks.AnyAsync(a => a.CategoryId == id);
		if (hasArtworks)
		{
			return BadRequest(new { message = "Category is used by artworks and cannot be deleted." });
		}

		_context.Categories.Remove(category);
		await _context.SaveChangesAsync();
		return Ok(new { message = "Category deleted." });
	}

	[HttpGet("tags")]
	public async Task<IActionResult> GetTags()
	{
		var tags = await _context.ArtworkTags
			.Where(t => !string.IsNullOrWhiteSpace(t.Tag))
			.Select(t => t.Tag.Trim())
			.Distinct()
			.OrderBy(t => t)
			.ToListAsync();

		return Ok(tags);
	}

	[HttpPost("tags")]
	public async Task<IActionResult> CreateTag([FromBody] TagUpsertRequest request)
	{
		var tag = request.Name?.Trim();
		if (string.IsNullOrWhiteSpace(tag))
		{
			return BadRequest(new { message = "Tag name is required." });
		}

		var exists = await _context.ArtworkTags.AnyAsync(t => t.Tag.ToLower() == tag.ToLower());
		if (exists)
		{
			return Conflict(new { message = "Tag already exists." });
		}

        var artworkTag = new ArtworkTag
        {
            Id = Guid.NewGuid(),
            ArtworkId = Guid.Empty, // Placeholder, will be ignored since tag is unique
            Tag = tag
        };
		_context.ArtworkTags.Add(artworkTag);
		await _context.SaveChangesAsync();

		return Ok(new
		{
			message = "Tag created.",
			tag
		});
	}

	[HttpPut("tags/{oldTag}")]
	public async Task<IActionResult> RenameTag(string oldTag, [FromBody] TagUpsertRequest request)
	{
		var oldName = oldTag?.Trim();
		var newName = request.Name?.Trim();

		if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName))
		{
			return BadRequest(new { message = "Tag names are required." });
		}

		if (string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
		{
			return Ok(new { message = "Tag renamed." });
		}

		var tagsToRename = await _context.ArtworkTags
			.Where(t => t.Tag.ToLower() == oldName.ToLower())
			.ToListAsync();

		if (tagsToRename.Count == 0)
		{
			return NotFound(new { message = "Tag not found." });
		}

		foreach (var item in tagsToRename)
		{
			item.Tag = newName;
		}

		try
		{
			await _context.SaveChangesAsync();
		}
		catch (DbUpdateException)
		{
			return Conflict(new { message = "Rename would create duplicate tag values on some artworks." });
		}

		return Ok(new { message = "Tag renamed." });
	}

	[HttpDelete("tags/{tag}")]
	public async Task<IActionResult> DeleteTag(string tag)
	{
		var normalized = tag?.Trim();
		if (string.IsNullOrWhiteSpace(normalized))
		{
			return BadRequest(new { message = "Tag name is required." });
		}

		var tagsToDelete = await _context.ArtworkTags
			.Where(t => t.Tag.ToLower() == normalized.ToLower())
			.ToListAsync();

		if (tagsToDelete.Count == 0)
		{
			return NotFound(new { message = "Tag not found." });
		}

		_context.ArtworkTags.RemoveRange(tagsToDelete);
		await _context.SaveChangesAsync();

		return Ok(new { message = "Tag deleted." });
	}

	public sealed class CategoryUpsertRequest
	{
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
	}

	public sealed class TagUpsertRequest
	{
		public string Name { get; set; } = string.Empty;
	}
}
