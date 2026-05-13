using System.Threading.Tasks;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared;

namespace YaungMel_POS.Domain.Features.Auth
{
    public interface IUserService
    {
        Task<PagedResult<UserResponse>> RegisterAsync(UserRegisterRequest request);
        Task<PagedResult<UserResponse>> UpdateAsync(int id, UserUpdateRequest request, int currentUserId);
        Task<PagedResult<UserResponse>> DeleteAsync(int id);
        Task<PagedResult<UserResponse>> ChangePasswordAsync(int id, ChangePasswordRequest request, int currentUserId);
        Task<PagedResult<List<UserDTO>>> GetAllAsync();
        bool IsValidMobileNum(string mobileNum);
    }
}
