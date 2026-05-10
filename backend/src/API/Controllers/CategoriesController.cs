using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>Category management</summary>
[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoriesService _service;

    public CategoriesController(ICategoriesService service) { _service = service; }

    [HttpGet]
    public async Task<IActionResult> ListCategories()
    {
        try { var result = await _service.ListCategories(); return Ok(result); }
        catch (Exception e) { return StatusCode(500, new { error = e.Message }); }
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryInput input)
    {
        try { var result = await _service.CreateCategory(input); return StatusCode(201, result); }
        catch (Exception e) { return StatusCode(500, new { error = e.Message }); }
    }
}
