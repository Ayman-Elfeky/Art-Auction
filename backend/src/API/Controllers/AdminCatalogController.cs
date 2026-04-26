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
		var artworkTags = await _context.ArtworkTags
			.Where(t => !string.IsNullOrWhiteSpace(t.Tag))
			.Select(t => t.Tag.Trim())
			.ToListAsync();

		var adminTags = await _context.AdminTags
			.Where(t => !string.IsNullOrWhiteSpace(t.Name))
			.Select(t => t.Name.Trim())
			.ToListAsync();

		var tags = artworkTags
			.Concat(adminTags)
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.OrderBy(t => t)
			.ToList();

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

		var exists = await _context.AdminTags.AnyAsync(t => t.Name.ToLower() == tag.ToLower());
		if (exists)
		{
			return Conflict(new { message = "Tag already exists." });
		}

		_context.AdminTags.Add(new AdminTag
		{
			Id = Guid.NewGuid(),
			Name = tag,
			CreatedAt = DateTime.UtcNow
		});
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

		var adminTag = await _context.AdminTags
			.FirstOrDefaultAsync(t => t.Name.ToLower() == oldName.ToLower());

		if (tagsToRename.Count == 0 && adminTag is null)
		{
			return NotFound(new { message = "Tag not found." });
		}

		foreach (var item in tagsToRename)
		{
			item.Tag = newName;
		}

		if (adminTag is not null)
		{
			adminTag.Name = newName;
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

		var adminTag = await _context.AdminTags
			.FirstOrDefaultAsync(t => t.Name.ToLower() == normalized.ToLower());

		if (tagsToDelete.Count == 0 && adminTag is null)
		{
			return NotFound(new { message = "Tag not found." });
		}

		if (tagsToDelete.Count > 0)
		{
			_context.ArtworkTags.RemoveRange(tagsToDelete);
		}

		if (adminTag is not null)
		{
			_context.AdminTags.Remove(adminTag);
		}

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
