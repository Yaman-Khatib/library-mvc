using Library.BL.Dtos;
using Library.BL.Results;

namespace Library.BL.Interfaces.Services;

public interface IAuthService
{
    Task<Result<AuthUserDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<AuthUserDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
}
