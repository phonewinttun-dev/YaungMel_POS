using System.ComponentModel.DataAnnotations;
using YaungMel_POS.database.Models;

namespace YaungMel_POS.domain.DTOs
{
    public class LoginRequest
    {
        [Required]
        public string MobileNum { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string MobileNum { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class UserRegisterRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string MobileNum { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public Tbl_User.UserRole Role { get; set; } = Tbl_User.UserRole.Staff;
    }

    public class UserResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? UserId { get; set; }
    }

    public class UserUpdateRequest
    {
        public string? Name { get; set; }
        public string? MobileNum { get; set; }
        public string? Password { get; set; }
        public Tbl_User.UserRole? Role { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required]
        public string OldPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
