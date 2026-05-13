using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared;

namespace YaungMel_POS.Domain.Features.Auth;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _registerService;

    public AuthController(IAuthService authService, IUserService registerService)
    {
        _authService = authService;
        _registerService = registerService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers([FromQuery] PaginationRequest request)
    {
        var result = await _registerService.GetAllAsync(request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    //[Authorize(Roles = "Admin,Staff")]
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(PagedResult<object>.SystemError("Invalid registration data."));

        var result = await _registerService.RegisterAsync(request);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid login data.");
        }

        var result = await _authService.LoginAsync(request);

        if (result == null)
        {
            return Unauthorized();
        }

        var tokenResponse = result.Data;

        // Store refresh token in HttpOnly cookie
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7) 
        };

        Response.Cookies.Append("refreshToken", tokenResponse.RefreshToken, cookieOptions);

        // Clear refresh token from response body
        tokenResponse.RefreshToken = string.Empty;

        return Ok(new
        {
            success = true,
            message = "Token refreshed successfully.",
            data = tokenResponse
        });
    }

    //[Authorize]
    //[HttpPut("users/{id}")]
    //public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateRequest request)
    //{
    //    if (!ModelState.IsValid)
    //        return BadRequest(ApiResponse<UserResponse>.Fail("Invalid update data."));

    //    var currentUserId = GetCurrentUserId();

    //    if (id != currentUserId) return Forbid();

    //    var result = await _registerService.UpdateAsync(id, request, currentUserId);

    //    if (!result.IsSuccess)
    //    {
    //        return BadRequest(result);
    //    }

    //    return Ok(result);
    //}

    //[Authorize]
    //[HttpPost("users/{id}/change-password")]
    //public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
    //{
    //    if (!ModelState.IsValid)
    //        return BadRequest(ApiResponse<UserResponse>.Fail("Invalid password change data."));

    //    var currentUserId = GetCurrentUserId();

    //    if (id != currentUserId) return Forbid();

    //    var result = await _registerService.ChangePasswordAsync(id, request, currentUserId);

    //    if (!result.IsSuccess)
    //    {
    //        return BadRequest(result);
    //    }

    //    return Ok(result);
    //}

    [Authorize(Roles = "Admin")]
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _registerService.DeleteAsync(id);

        if (!result.IsSuccess) return BadRequest(result);

        return Ok(result);
    }

    
}
