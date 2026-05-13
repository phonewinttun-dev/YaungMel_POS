using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared;

namespace YaungMel_POS.Domain.Features.Point;

public class DisabledPointService : IPointService
{
    public Task<PagedResult<CreateAccountResDTO>> CreateAccountAsync(CreateAccountReqDTO request)
        => Task.FromResult(PagedResult<CreateAccountResDTO>.SystemError("Point system is disabled"));

    public Task<PagedResult<AccountListResponseWrapper>> GetAccountsAsync(AccountListReqDTO request)
        => Task.FromResult(PagedResult<AccountListResponseWrapper>.SystemError("Point system is disabled"));
    public Task<PagedResult<AccountLookupResponse>> LookupAccountAsync(string userId)
        => Task.FromResult(PagedResult<AccountLookupResponse>.SystemError("Point system is disabled"));
    public Task<PagedResult<CheckBalanceResDTO>> GetUserBalanceAsync(CheckBalanceReqDTO request)
        => Task.FromResult(PagedResult<CheckBalanceResDTO>.SystemError("Point system is disabled"));

    public Task<PagedResult<EarnPointResDTO>> EarnPointsAsync(EarnPointReqDTO request)
        => Task.FromResult(PagedResult<EarnPointResDTO>.SystemError("Point system is disabled"));

    public Task<PagedResult<List<AvailableRewardResDTO>>> GetAvailableRewardsAsync()
        => Task.FromResult(PagedResult<List<AvailableRewardResDTO>>.SystemError("Point system is disabled"));

    public Task<PagedResult<ClaimRewardResDTO>> ClaimRewardAsync(ClaimRewardReqDTO request)
        => Task.FromResult(PagedResult<ClaimRewardResDTO>.SystemError("Point system is disabled"));

    public Task<PagedResult<List<PointHistoryResDTO>>> GetPointHistoryAsync(string accountId)
        => Task.FromResult(PagedResult<List<PointHistoryResDTO>>.SystemError("Point system is disabled"));

    public Task<PagedResult<List<PendingRedemptionResDTO>>> GetPendingRedemptionsAsync()
        => Task.FromResult(PagedResult<List<PendingRedemptionResDTO>>.SystemError("Point system is disabled"));

    public Task<PagedResult<bool>> UpdateRedemptionStatusAsync(string redemptionId, RedemptionStatus status)
        => Task.FromResult(PagedResult<bool>.SystemError("Point system is disabled"));

    public Task<PagedResult<CreateRewardResDTO>> CreateRewardAsync(CreateRewardReqDTO request)
        => Task.FromResult(PagedResult<CreateRewardResDTO>.SystemError("Point system is disabled"));

    public Task<PagedResult<AvailableRewardResDTO>> UpdateRewardAsync(string id, UpdateRewardReqDTO request)
        => Task.FromResult(PagedResult<AvailableRewardResDTO>.SystemError("Point system is disabled"));

    public Task<PagedResult<bool>> DeleteRewardAsync(string id)
        => Task.FromResult(PagedResult<bool>.SystemError("Point system is disabled"));
}
