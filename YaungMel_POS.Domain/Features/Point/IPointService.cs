using YaungMel_POS.domain.DTOs;
using YaungMel_POS.shared;
using YaungMel_POS.shared.Responses;

namespace YaungMel_POS.domain.Features.Point
{
    public interface IPointService
    {
        Task<Result<ClaimRewardResDTO>> ClaimRewardAsync(ClaimRewardReqDTO request);
        Task<Result<CreateAccountResDTO>> CreateAccountAsync(CreateAccountReqDTO request);
        Task<Result<AccountLookupResponse>> LookupAccountAsync(string userId);
        Task<Result<EarnPointResDTO>> EarnPointsAsync(EarnPointReqDTO request);
        Task<Result<AccountListResponseWrapper>> GetAccountsAsync(AccountListReqDTO request);
        Task<Result<List<AvailableRewardResDTO>>> GetAvailableRewardsAsync();
        Task<Result<List<PendingRedemptionResDTO>>> GetPendingRedemptionsAsync();
        Task<Result<List<PointHistoryResDTO>>> GetPointHistoryAsync(string accountId);
        Task<Result<CheckBalanceResDTO>> GetUserBalanceAsync(CheckBalanceReqDTO request);
        Task<Result<bool>> UpdateRedemptionStatusAsync(string redemptionId, RedemptionStatus status);
    }
}