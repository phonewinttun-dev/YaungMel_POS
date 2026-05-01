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
    public Task<ApiResponse<CreateAccountResDTO>> CreateAccountAsync(CreateAccountReqDTO request)
        => Task.FromResult(ApiResponse<CreateAccountResDTO>.Fail("Point system is disabled"));

    public Task<ApiResponse<AccountListResponseWrapper>> GetAccountsAsync(AccountListReqDTO request)
        => Task.FromResult(ApiResponse<AccountListResponseWrapper>.Fail("Point system is disabled"));
    public Task<ApiResponse<AccountLookupResponse>> LookupAccountAsync(string userId)
        => Task.FromResult(ApiResponse<AccountLookupResponse>.Fail("Point system is disabled"));
    public Task<ApiResponse<CheckBalanceResDTO>> GetUserBalanceAsync(CheckBalanceReqDTO request)
        => Task.FromResult(ApiResponse<CheckBalanceResDTO>.Fail("Point system is disabled"));

    public Task<ApiResponse<EarnPointResDTO>> EarnPointsAsync(EarnPointReqDTO request)
        => Task.FromResult(ApiResponse<EarnPointResDTO>.Fail("Point system is disabled"));

    public Task<ApiResponse<List<AvailableRewardResDTO>>> GetAvailableRewardsAsync()
        => Task.FromResult(ApiResponse<List<AvailableRewardResDTO>>.Fail("Point system is disabled"));

    public Task<ApiResponse<ClaimRewardResDTO>> ClaimRewardAsync(ClaimRewardReqDTO request)
        => Task.FromResult(ApiResponse<ClaimRewardResDTO>.Fail("Point system is disabled"));

    public Task<ApiResponse<List<PointHistoryResDTO>>> GetPointHistoryAsync(string accountId)
        => Task.FromResult(ApiResponse<List<PointHistoryResDTO>>.Fail("Point system is disabled"));

    public Task<ApiResponse<List<PendingRedemptionResDTO>>> GetPendingRedemptionsAsync()
        => Task.FromResult(ApiResponse<List<PendingRedemptionResDTO>>.Fail("Point system is disabled"));

    public Task<ApiResponse<bool>> UpdateRedemptionStatusAsync(string redemptionId, RedemptionStatus status)
        => Task.FromResult(ApiResponse<bool>.Fail("Point system is disabled"));
}
