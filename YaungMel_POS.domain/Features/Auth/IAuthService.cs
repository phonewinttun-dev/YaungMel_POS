using YaungMel_POS.domain.DTOs;

namespace YaungMel_POS.domain.Features.Auth;

public interface IAuthService
{
    Task<TokenResponse?> LoginAsync(LoginRequest request);
    Task<TokenResponse?> RefreshTokenAsync(string refreshToken);
}
