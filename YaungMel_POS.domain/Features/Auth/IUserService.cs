using System.Threading.Tasks;
using YaungMel_POS.domain.DTOs;
using YaungMel_POS.shared.Responses;

namespace YaungMel_POS.domain.Features.Auth
{
    public interface IUserService
    {
        Task<ApiResponse<UserResponse>> RegisterAsync(UserRegisterRequest request);
        Task<ApiResponse<UserResponse>> UpdateAsync(int id, UserUpdateRequest request, int currentUserId);
        Task<ApiResponse<UserResponse>> DeleteAsync(int id);
        Task<ApiResponse<UserResponse>> ChangePasswordAsync(int id, ChangePasswordRequest request, int currentUserId);
        bool IsValidMobileNum(string mobileNum);
    }
}
