using System.Security.Claims;
using YaungMel_POS.database.Models;

namespace YaungMel_POS.domain.Features.Auth;

public interface ITokenService
{
    string GenerateAccessToken(Tbl_User user);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
