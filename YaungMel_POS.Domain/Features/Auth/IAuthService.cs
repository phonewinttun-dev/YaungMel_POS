using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared;

namespace YaungMel_POS.Domain.Features.Auth;

public interface IAuthService
{
    Task<Result<TokenResponse?>> LoginAsync(LoginRequest request);
    Task<Result<TokenResponse?>> RefreshTokenAsync(string refreshToken);
}
