using System.Threading.Tasks;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared;

namespace YaungMel_POS.Domain.Features.Auth
{
    public interface IUserService
    {
        Task<Result<UserResponse>> RegisterAsync(UserRegisterRequest request);
        //Task<Result<UserResponse>> UpdateAsync(int id, UserUpdateRequest request, int currentUserId);
        Task<Result<UserResponse>> DeleteAsync(int id);
        //Task<Result<UserResponse>> ChangePasswordAsync(int id, ChangePasswordRequest request, int currentUserId);
        Task<PagedResult<UserDTO>> GetAllAsync(PaginationRequest request);
        bool IsValidMobileNum(string mobileNum);
    }
}
