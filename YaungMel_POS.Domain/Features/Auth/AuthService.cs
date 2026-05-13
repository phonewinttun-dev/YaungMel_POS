using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using YaungMel_POS.Database.Models;
using System.Security.Cryptography;
using System.Text;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Database.Data;
using YaungMel_POS.Shared;

namespace YaungMel_POS.Domain.Features.Auth;

public class AuthService : IAuthService
{
    private readonly POSDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthService(POSDbContext context, ITokenService tokenService, IConfiguration configuration)
    {
        _context = context;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    private IQueryable<Tbl_User> ActiveUserQuery => _context.Users
            .AsNoTracking().Where(u => !u.DeleteFlag);

    #region user login
    public async Task<Result<TokenResponse?>> LoginAsync(LoginRequest request)
    {
        var user = await ActiveUserQuery
            .FirstOrDefaultAsync(u => u.MobileNum == request.MobileNum && !u.DeleteFlag);

        if (user == null)
        {
            return Result<TokenResponse?>.NotFound("Invalid username or password!");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            return Result<TokenResponse?>.ValidationError("Invalid username or password!");
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var expiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");
        var userToken = new Tbl_User_Token
        {
            UserId = user.Id,
            TokenHash = HashToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            CreatedAt = DateTime.UtcNow,
            Revoked = false
        };

        _context.UserToken.Add(userToken);
        await _context.SaveChangesAsync();

        return Result<TokenResponse?>.Success(new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            MobileNum = user.MobileNum,
            Role = user.Role.ToString()
        });
    }
    #endregion

    #region refresh token
    public async Task<Result<TokenResponse?>> RefreshTokenAsync(string refreshToken)
    {
        var hashedToken = HashToken(refreshToken);
        var userToken = await _context.UserToken
            .Include(ut => ut.User)
            .FirstOrDefaultAsync(ut => ut.TokenHash == hashedToken && !ut.Revoked && ut.ExpiresAt > DateTime.UtcNow);

        if (userToken == null)
        {
            return Result<TokenResponse?>.NotFound("Unexpected error occured!");
        }

        userToken.Revoked = true;

        var user = userToken.User;
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        var expiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");
        var newUserToken = new Tbl_User_Token
        {
            UserId = user.Id,
            TokenHash = HashToken(newRefreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            CreatedAt = DateTime.UtcNow,
            Revoked = false
        };

        _context.UserToken.Add(newUserToken);
        await _context.SaveChangesAsync();

        return Result<TokenResponse?>.Success(new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            MobileNum = user.MobileNum,
            Role = user.Role.ToString()
        });
    }
    #endregion

    private string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashedBytes);
    }
}
