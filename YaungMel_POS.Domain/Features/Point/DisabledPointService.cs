using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YaungMel_POS.domain.DTOs;
using YaungMel_POS.shared;
using YaungMel_POS.shared.Responses;

namespace YaungMel_POS.domain.Features.Point;

public class DisabledPointService : IPointService
{
    public Task<Result<CreateAccountResDTO>> CreateAccountAsync(CreateAccountReqDTO request)
        => Task.FromResult(Result<CreateAccountResDTO>.SystemError("Point system is disabled"));

    public Task<Result<AccountListResponseWrapper>> GetAccountsAsync(AccountListReqDTO request)
        => Task.FromResult(Result<AccountListResponseWrapper>.SystemError("Point system is disabled"));
    public Task<Result<AccountLookupResponse>> LookupAccountAsync(string userId)
        => Task.FromResult(Result<AccountLookupResponse>.SystemError("Point system is disabled"));
    public Task<Result<CheckBalanceResDTO>> GetUserBalanceAsync(CheckBalanceReqDTO request)
        => Task.FromResult(Result<CheckBalanceResDTO>.SystemError("Point system is disabled"));

    public Task<Result<EarnPointResDTO>> EarnPointsAsync(EarnPointReqDTO request)
        => Task.FromResult(Result<EarnPointResDTO>.SystemError("Point system is disabled"));

    public Task<Result<List<AvailableRewardResDTO>>> GetAvailableRewardsAsync()
        => Task.FromResult(Result<List<AvailableRewardResDTO>>.SystemError("Point system is disabled"));

    public Task<Result<ClaimRewardResDTO>> ClaimRewardAsync(ClaimRewardReqDTO request)
        => Task.FromResult(Result<ClaimRewardResDTO>.SystemError("Point system is disabled"));

    public Task<Result<List<PointHistoryResDTO>>> GetPointHistoryAsync(string accountId)
        => Task.FromResult(Result<List<PointHistoryResDTO>>.SystemError("Point system is disabled"));

    public Task<Result<List<PendingRedemptionResDTO>>> GetPendingRedemptionsAsync()
        => Task.FromResult(Result<List<PendingRedemptionResDTO>>.SystemError("Point system is disabled"));

    public Task<Result<bool>> UpdateRedemptionStatusAsync(string redemptionId, RedemptionStatus status)
        => Task.FromResult(Result<bool>.SystemError("Point system is disabled"));
}
