using Library.BL.Dtos;
using Library.BL.Entities;
using Library.BL.Interfaces.Repositories;
using Library.BL.Interfaces.Services;
using Library.BL.Results;
using Microsoft.AspNetCore.Identity;

namespace Library.BL.Services;

public sealed class AuthService : IAuthService
{
    private static readonly object PasswordHasherUser = new();
    private readonly IUserRepository _userRepository;
    private readonly PasswordHasher<object> _passwordHasher;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = new PasswordHasher<object>();
    }

    public async Task<Result<AuthUserDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        if (normalizedEmail is null)
        {
            return Result<AuthUserDto>.Fail(ErrorCodes.ValidationError, "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            return Result<AuthUserDto>.Fail(ErrorCodes.ValidationError, "Password must be at least 8 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            return Result<AuthUserDto>.Fail(ErrorCodes.ValidationError, "FirstName is required.");
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            return Result<AuthUserDto>.Fail(ErrorCodes.ValidationError, "LastName is required.");
        }

        if (request.DateOfBirth == default)
        {
            return Result<AuthUserDto>.Fail(ErrorCodes.ValidationError, "DateOfBirth is required.");
        }

        var existing = await _userRepository.GetAuthByEmailAsync(normalizedEmail, cancellationToken);
        if (existing is not null)
        {
            return Result<AuthUserDto>.Fail(ErrorCodes.ValidationError, "Email is already registered.");
        }

        var passwordHash = _passwordHasher.HashPassword(PasswordHasherUser, request.Password);
        var user = new User
        {
            Id = 0,
            Email = normalizedEmail,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            DateOfBirth = request.DateOfBirth,
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
        };

        var id = await _userRepository.AddWithPasswordHashAsync(user, passwordHash, cancellationToken);
        return Result<AuthUserDto>.Success(new AuthUserDto
        {
            Id = id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
        });
    }

    public async Task<Result<AuthUserDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        if (normalizedEmail is null)
        {
            return Result<AuthUserDto>.Fail(ErrorCodes.ValidationError, "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<AuthUserDto>.Fail(ErrorCodes.ValidationError, "Password is required.");
        }

        var record = await _userRepository.GetAuthByEmailAsync(normalizedEmail, cancellationToken);
        if (record is null)
        {
            return Result<AuthUserDto>.Fail(ErrorCodes.ValidationError, "Invalid email or password.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(PasswordHasherUser, record.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return Result<AuthUserDto>.Fail(ErrorCodes.ValidationError, "Invalid email or password.");
        }

        return Result<AuthUserDto>.Success(new AuthUserDto
        {
            Id = record.Id,
            Email = record.Email,
            FirstName = record.FirstName,
            LastName = record.LastName,
            Role = record.Role,
        });
    }

    private static string? NormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var normalized = email.Trim().ToLowerInvariant();
        if (normalized.Length > 100)
        {
            normalized = normalized[..100];
        }

        return normalized;
    }
}
