using ArtAuction.Application.Features.Auth.Commands.Login;
using ArtAuction.Application.Features.Auth.Commands.Register;
using ArtAuction.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ArtAuction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Register a new user (Artist or Buyer)
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var command = new RegisterCommand
        {
            Username = dto.Username,
            Email = dto.Email,
            Password = dto.Password,
            Role = dto.Role
        };

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Login and get JWT token
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var command = new LoginCommand
        {
            Email = dto.Email,
            Password = dto.Password
        };

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            return Unauthorized(result.Error);
        }

        return Ok(result.Data);
    }
}
