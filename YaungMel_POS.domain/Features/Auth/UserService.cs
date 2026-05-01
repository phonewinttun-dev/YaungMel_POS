using Microsoft.EntityFrameworkCore;
using YaungMel_POS.database.Models;
using System.Security.Claims;
using System.Text.RegularExpressions;
using YaungMel_POS.database.Data;
using YaungMel_POS.domain.DTOs;
using YaungMel_POS.shared.Responses;

namespace YaungMel_POS.domain.Features.Auth
{
    public class UserService : IUserService
    {
        private readonly POSDbContext _context;

        public UserService(POSDbContext context)
        {
            _context = context;
        }

        #region mobile number and password validation
        public bool IsValidMobileNum(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Strip spaces and dashes
            string clean = phone.Replace(" ", "").Replace("-", "");

            if (clean.Length < 7 || clean.Length > 15)
                return false;

            for (int i = 0; i < clean.Length; i++)
            {
                char c = clean[i];

                if (i == 0 && c == '+')
                    continue;

                if (!char.IsDigit(c))
                    return false;
            }

            return true;
        }

        public (bool IsValid, string Message) ValidatePassword(string password)
        {
            int passLen = password.Length;

            if (passLen < 12 || passLen > 24)
            {
                return (false, $"password must be between 12 and 24 characters (got {passLen})");
            }
            return (true, string.Empty);
        }
        #endregion

        #region user registration
        public async Task<ApiResponse<UserResponse>> RegisterAsync(UserRegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name)) return ApiResponse<UserResponse>.Fail("Username cannot be null");

            // mobile number validation check
            var mobileNum = request.MobileNum.Trim();

            if (string.IsNullOrWhiteSpace(request.MobileNum)) return ApiResponse<UserResponse>.Fail("Mobile number is required.");

            if (!IsValidMobileNum(mobileNum)) return ApiResponse<UserResponse>.Fail("Invalid mobile number format.");

            // password validation check
            if (string.IsNullOrWhiteSpace(request.Password)) return ApiResponse<UserResponse>.Fail("Password required.");

            var passValidation = ValidatePassword(request.Password);

            if (!passValidation.IsValid) return ApiResponse<UserResponse>.Fail(passValidation.Message);

            // duplication check
            var existingUser = await _context.Users
                .AnyAsync(u => u.MobileNum == mobileNum && !u.DeleteFlag);

            if (existingUser) return ApiResponse<UserResponse>.Fail("User with this mobile number already exists.");
            
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new Tbl_User
            {
                Name = request.Name.Trim(),
                MobileNum = request.MobileNum.Trim(),
                Password = hashedPassword,
                Role = request.Role,
                CreatedAt = DateTime.UtcNow,
                DeleteFlag = false
            };

            try
            {
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                var responseJson = new UserResponse
                {
                    IsSuccess = true,
                    Message = "User registered successfully.",
                    UserId = newUser.Id
                };

                return ApiResponse<UserResponse>.Success(responseJson, responseJson.Message);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserResponse>.Fail($"An error occurred during registration: {ex.Message}");
            }
        }
        #endregion

        #region edit user profile
        public async Task<ApiResponse<UserResponse>> UpdateAsync(int id, UserUpdateRequest request,int currentUserId)
        {
            // current user check
            if (id != currentUserId) return ApiResponse<UserResponse>.Fail("Unauthorized access!");

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.DeleteFlag);

                if (user == null)
                    return ApiResponse<UserResponse>.Fail("User not found.");

                if (!string.IsNullOrWhiteSpace(request.Name))
                    user.Name = request.Name.Trim();

                if (!string.IsNullOrWhiteSpace(request.MobileNum))
                {
                    if (!IsValidMobileNum(request.MobileNum))
                        return ApiResponse<UserResponse>.Fail("Invalid mobile number format.");

                    var mobileExists = await _context.Users
                        .AnyAsync(u => u.MobileNum == request.MobileNum.Trim() && u.Id != id && !u.DeleteFlag);

                    if (mobileExists)
                        return ApiResponse<UserResponse>.Fail("Mobile number already in use by another user.");

                    user.MobileNum = request.MobileNum.Trim();
                }

                //if (request.Role.HasValue)
                //    user.Role = request.Role.Value;

                //if (!string.IsNullOrWhiteSpace(request.Password))
                //{
                //    user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
                //}

                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var response = new UserResponse
                {
                    IsSuccess = true,
                    Message = "User updated successfully.",
                    UserId = user.Id
                };

                return ApiResponse<UserResponse>.Success(response, response.Message);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserResponse>.Fail($"An error occurred during update: {ex.Message}");
            }
        }
        #endregion

        #region change password
        public async Task<ApiResponse<UserResponse>> ChangePasswordAsync(int id, ChangePasswordRequest request,int currentUserId)
        {
            // current user check
            if (id != currentUserId) return ApiResponse<UserResponse>.Fail("Unauthorized access!");

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.DeleteFlag);

                if (user == null)
                    return ApiResponse<UserResponse>.Fail("User not found.");

                if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password))
                    return ApiResponse<UserResponse>.Fail("Invalid old password.");
             
                var passValidation = ValidatePassword(request.NewPassword);
                if (!passValidation.IsValid) return ApiResponse<UserResponse>.Fail(passValidation.Message);

                user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var response = new UserResponse
                {
                    IsSuccess = true,
                    Message = "Password changed successfully.",
                    UserId = user.Id
                };

                return ApiResponse<UserResponse>.Success(response, response.Message);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserResponse>.Fail($"An error occurred while changing password: {ex.Message}");
            }
        }
        #endregion

        #region delete user
        public async Task<ApiResponse<UserResponse>> DeleteAsync(int id)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

                if (user == null || user.DeleteFlag)
                    return ApiResponse<UserResponse>.Fail("User not found or already deleted.");

                user.DeleteFlag = true;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var response = new UserResponse
                {
                    IsSuccess = true,
                    Message = "User deleted successfully.",
                    UserId = user.Id
                };

                return ApiResponse<UserResponse>.Success(response, response.Message);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserResponse>.Fail($"An error occurred during deletion: {ex.Message}");
            }
        }
        #endregion

    }
}
