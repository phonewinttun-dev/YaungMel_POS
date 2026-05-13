using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared;

namespace YaungMel_POS.Domain.Features.Point
{
    public interface IPointService
    {
        Task<PagedResult<ClaimRewardResDTO>> ClaimRewardAsync(ClaimRewardReqDTO request);
        Task<PagedResult<CreateAccountResDTO>> CreateAccountAsync(CreateAccountReqDTO request);
        Task<PagedResult<AccountLookupResponse>> LookupAccountAsync(string userId);
        Task<PagedResult<EarnPointResDTO>> EarnPointsAsync(EarnPointReqDTO request);
        Task<PagedResult<AccountListResponseWrapper>> GetAccountsAsync(AccountListReqDTO request);
        Task<PagedResult<List<AvailableRewardResDTO>>> GetAvailableRewardsAsync();
        Task<PagedResult<List<PendingRedemptionResDTO>>> GetPendingRedemptionsAsync();
        Task<PagedResult<List<PointHistoryResDTO>>> GetPointHistoryAsync(string accountId);
        Task<PagedResult<CheckBalanceResDTO>> GetUserBalanceAsync(CheckBalanceReqDTO request);
        Task<PagedResult<bool>> UpdateRedemptionStatusAsync(string redemptionId, RedemptionStatus status);
        Task<PagedResult<CreateRewardResDTO>> CreateRewardAsync(CreateRewardReqDTO request);
        Task<PagedResult<AvailableRewardResDTO>> UpdateRewardAsync(string id, UpdateRewardReqDTO request);
        Task<PagedResult<bool>> DeleteRewardAsync(string id);
    }
}