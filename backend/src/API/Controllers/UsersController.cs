using Api.Models;
using Api.Services;
using ArtAuction.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>User management</summary>
[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
[Authorize(Policy = Permissions.UsersView)]
public class UsersController : ControllerBase
{
    private readonly IUsersService _service;

    public UsersController(IUsersService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> ListUsers([FromQuery] Dictionary<string, string> query)
    {
        try
        {
            var result = await _service.ListUsers(query);
            return Ok(result);
        }
        catch (Exception e)
        {
            return StatusCode(500, new { error = e.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser([FromRoute] string Id)
    {
        try
        {
            var result = await _service.GetUser(Id);
            return Ok(result);
        }
        catch (Exception e)
        {
            return StatusCode(500, new { error = e.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserInput input)
    {
        try
        {
            var result = await _service.CreateUser(input);
            return StatusCode(201, result);
        }
        catch (Exception e)
        {
            return StatusCode(500, new { error = e.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser([FromRoute] string Id, [FromBody] UpdateUserInput input)
    {
        try
        {
            var result = await _service.UpdateUser(Id, input);
            return Ok(result);
        }
        catch (Exception e)
        {
            return StatusCode(500, new { error = e.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser([FromRoute] string Id)
    {
        try
        {
            var result = await _service.DeleteUser(Id);
            return Ok(result);
        }
        catch (Exception e)
        {
            return StatusCode(500, new { error = e.Message });
        }
    }
}
