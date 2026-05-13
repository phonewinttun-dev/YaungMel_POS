using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YaungMel_POS.Database.Data;
using YaungMel_POS.Database.Models;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared;

namespace YaungMel_POS.Domain.Features.Auth
{
    public class UserService : IUserService
    {
        private readonly POSDbContext _context;

        public UserService(POSDbContext context)
        {
            _context = context;
        }

        private IQueryable<Tbl_User> ActiveUserQuery => _context.Users
            .AsNoTracking().Where(u => !u.DeleteFlag);

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
        public async Task<Result<UserResponse>> RegisterAsync(UserRegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name)) return Result<UserResponse>.ValidationError("Username cannot be null");

            // mobile number validation check
            var mobileNum = request.MobileNum.Trim();
            if (string.IsNullOrWhiteSpace(request.MobileNum)) return Result<UserResponse>.ValidationError("Mobile number is required.");
            if (!IsValidMobileNum(mobileNum)) return Result<UserResponse>.ValidationError("Invalid mobile number format.");

            // password validation check
            if (string.IsNullOrWhiteSpace(request.Password)) return Result<UserResponse>.SystemError("Password required.");
            var passwordValidation = ValidatePassword(request.Password);
            if (!passwordValidation.IsValid) return Result<UserResponse>.ValidationError(passwordValidation.Message);

            // user duplication check
            var existingUser = await _context.Users.AnyAsync(u => u.MobileNum == mobileNum && !u.DeleteFlag);
            if (existingUser) return Result<UserResponse>.ValidationError("User with this mobile number already exists.");
            
            // user creation
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

                return Result<UserResponse>.Success(responseJson, responseJson.Message);
            }
            catch (Exception ex)
            {
                return Result<UserResponse>.SystemError($"An error occurred during registration: {ex.Message}");
            }
        }
        #endregion

        #region edit user profile
        //public async Task<Result<UserResponse>> UpdateAsync(int id, UserUpdateRequest request,int currentUserId)
        //{
        //    // current user check
        //    if (id != currentUserId) return Result<UserResponse>.ValidationError("Unauthorized access!");

        //    try
        //    {
        //        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.DeleteFlag);

        //        if (user == null) return Result<UserResponse>.SystemError("User not found.");

        //        if (!string.IsNullOrWhiteSpace(request.Name))
        //            user.Name = request.Name.Trim();

        //        if (!string.IsNullOrWhiteSpace(request.MobileNum))
        //        {
        //            if (!IsValidMobileNum(request.MobileNum))
        //                return Result<UserResponse>.SystemError("Invalid mobile number format.");

        //            var mobileExists = await _context.Users
        //                .AnyAsync(u => u.MobileNum == request.MobileNum.Trim() && u.Id != id && !u.DeleteFlag);

        //            if (mobileExists)
        //                return Result<UserResponse>.SystemError("Mobile number already in use by another user.");

        //            user.MobileNum = request.MobileNum.Trim();
        //        }

        //        //if (request.Role.HasValue)
        //        //    user.Role = request.Role.Value;

        //        //if (!string.IsNullOrWhiteSpace(request.Password))
        //        //{
        //        //    user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        //        //}

        //        user.UpdatedAt = DateTime.UtcNow;

        //        await _context.SaveChangesAsync();

        //        var response = new UserResponse
        //        {
        //            IsSuccess = true,
        //            Message = "User updated successfully.",
        //            UserId = user.Id
        //        };

        //        return Result<UserResponse>.Success(response, response.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Result<UserResponse>.SystemError($"An error occurred during update: {ex.Message}");
        //    }
        //}
        #endregion

        #region change password
        //public async Task<PagedResult<UserResponse>> ChangePasswordAsync(int id, ChangePasswordRequest request,int currentUserId)
        //{
        //    // current user check
        //    if (id != currentUserId) return PagedResult<UserResponse>.SystemError("Unauthorized access!");

        //    try
        //    {
        //        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.DeleteFlag);

        //        if (user == null)
        //            return PagedResult<UserResponse>.SystemError("User not found.");

        //        if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password))
        //            return PagedResult<UserResponse>.SystemError("Invalid old password.");
             
        //        var passValidation = ValidatePassword(request.NewPassword);
        //        if (!passValidation.IsValid) return PagedResult<UserResponse>.SystemError(passValidation.Message);

        //        user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        //        user.UpdatedAt = DateTime.UtcNow;

        //        await _context.SaveChangesAsync();

        //        var response = new UserResponse
        //        {
        //            IsSuccess = true,
        //            Message = "Password changed successfully.",
        //            UserId = user.Id
        //        };

        //        return PagedResult<UserResponse>.Success(response, response.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        return PagedResult<UserResponse>.SystemError($"An error occurred while changing password: {ex.Message}");
        //    }
        //}
        #endregion

        #region delete user
        public async Task<Result<UserResponse>> DeleteAsync(int id)
        {
            try
            {
                var user = await ActiveUserQuery.FirstOrDefaultAsync(u => u.Id == id);

                if (user == null || user.DeleteFlag)
                    return Result<UserResponse>.NotFound("User not found or already deleted.");

                user.DeleteFlag = true;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var response = new UserResponse
                {
                    IsSuccess = true,
                    Message = "User deleted successfully.",
                    UserId = user.Id
                };

                return Result<UserResponse>.Success(response, response.Message);
            }
            catch (Exception ex)
            {
                return Result<UserResponse>.SystemError($"An error occurred during deletion: {ex.Message}");
            }
        }
        #endregion
        
        #region get all users
        public async Task<PagedResult<UserDTO>> GetAllAsync(PaginationRequest request)
        {
            if (request == null)
                return PagedResult<UserDTO>.ValidationError("Page size must be greater than zero");

            try
            {
                var totalUsers = await ActiveUserQuery.CountAsync();

                var users = await ActiveUserQuery
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(u => new UserDTO
                    {
                        Id = u.Id,
                        Name = u.Name,
                        MobileNum = u.MobileNum,
                        Role = u.Role,
                        CreatedAt = u.CreatedAt
                    })
                    .ToListAsync();

                var pagination = new Pagination(request.PageNumber, request.PageSize, totalUsers);

                return PagedResult<UserDTO>.Success(users, pagination, "Users retrieved successfully.");
            }
            catch (Exception ex)
            {
                return PagedResult<UserDTO>.SystemError($"An error occurred while retrieving users: {ex.Message}");
            }
        }
        #endregion

    }
}
