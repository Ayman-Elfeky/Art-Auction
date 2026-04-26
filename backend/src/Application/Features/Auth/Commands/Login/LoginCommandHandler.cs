using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(IApplicationDbContext context, IJwtService jwtService, IPasswordHasher passwordHasher)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<AuthResponseDto>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
            return Result<AuthResponseDto>.Failure("Invalid email or password.");

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordSalt, user.PasswordHash))
            return Result<AuthResponseDto>.Failure("Invalid email or password.");

        // Check if account is active
        if (!user.IsActive)
            return Result<AuthResponseDto>.Failure("Your account has been deactivated.");

        var token = await _jwtService.GenerateToken(user, cancellationToken);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsApproved = user.IsApproved
        });
    }
}