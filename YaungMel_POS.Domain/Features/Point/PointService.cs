using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using YaungMel_POS.domain.DTOs;
using YaungMel_POS.shared;
using YaungMel_POS.shared.Responses;

namespace YaungMel_POS.domain.Features.Point;

public class PointService : IPointService
{
    private readonly HttpClient _client;

    public PointService(HttpClient clitent)
    {
        _client = clitent;
    }

    #region Account
    public async Task<Result<CreateAccountResDTO>> CreateAccountAsync(CreateAccountReqDTO request)
    {
        try
        {
            CreateAccount send = new CreateAccount
            {
                SystemId = "YaungMel",
                ExternalUserId = "YMP-" + request.Mobile,
                Tier = request.Tier,
                Mobile = "09" + request.Mobile,
                Email = request.Email
            };
            var response = await _client.PostAsJsonAsync("accounts", send);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CreateAccountResDTO>();

                return result != null
                    ? Result<CreateAccountResDTO>.Success(result, "Account created successfully.")
                    : Result<CreateAccountResDTO>.SystemError("Failed to parse created account data.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return Result<CreateAccountResDTO>.SystemError($"Creation Failed: {response.StatusCode} - {errorContent}");
        }
        catch (Exception ex)
        {
            return Result<CreateAccountResDTO>.SystemError($"Internal Error: {ex.Message}");
        }
    }
    public async Task<Result<AccountListResponseWrapper>> GetAccountsAsync(AccountListReqDTO request)
    {
        try
        {
            var queryParams = new Dictionary<string, string?>
            {
                ["Page"] = request.Page.ToString(),
                ["PageSize"] = request.PageSize.ToString(),
                ["SystemId"] = "YaungMel",
                ["SearchTerm"] = request.SearchTerm
            };

            if (request.Tier.HasValue)
            {
                queryParams["Tier"] = request.Tier.Value.ToString();
            }

            var url = QueryHelpers.AddQueryString("accounts", queryParams);

            var response = await _client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AccountListResponseWrapper>();
                return result != null
                    ? Result<AccountListResponseWrapper>.Success(result)
                    : Result<AccountListResponseWrapper>.SystemError("No account data found.");
            }

            return Result<AccountListResponseWrapper>.SystemError($"Error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return Result<AccountListResponseWrapper>.SystemError($"Internal Error: {ex.Message}");
        }
    }
    public async Task<Result<AccountLookupResponse>> LookupAccountAsync(string  userId)
    {
        try
        {
            var url = $"accounts/lookup/{userId}";

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await _client.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AccountLookupResponse>();
                return result != null
                    ? Result<AccountLookupResponse>.Success(result)
                    : Result<AccountLookupResponse>.SystemError("Account detail not found.");
            }

            return Result<AccountLookupResponse>.SystemError($"Error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return Result<AccountLookupResponse>.SystemError($"Internal Error: {ex.Message}");
        }
    }

    public async Task<Result<CheckBalanceResDTO>> GetUserBalanceAsync(CheckBalanceReqDTO request)
    {
        try
        {

            string url = $"accounts/lookup/{request.SystemId}/{request.ExternalUserId}";

            var response = await _client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<CheckBalanceResDTO>();
                return data != null
                    ? Result<CheckBalanceResDTO>.Success(data)
                    : Result<CheckBalanceResDTO>.SystemError("Data not found.");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Result<CheckBalanceResDTO>.SystemError("User not found in the system.");
            }

            return Result<CheckBalanceResDTO>.SystemError($"Error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return Result<CheckBalanceResDTO>.SystemError($"Internal Error: {ex.Message}");
        }
    }
    #endregion

    #region Earn and Use
    public async Task<Result<EarnPointResDTO>> EarnPointsAsync(EarnPointReqDTO request)
    {
        try
        {
            var response = await _client.PostAsJsonAsync("events/process", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<EarnPointResDTO>();

                return result != null
                    ? Result<EarnPointResDTO>.Success(result, "Points earned successfully.")
                    : Result<EarnPointResDTO>.SystemError("Failed to parse response data.");
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            return Result<EarnPointResDTO>.SystemError($"API Error: {response.StatusCode} - {errorMessage}");
        }
        catch (Exception ex)
        {
            return Result<EarnPointResDTO>.SystemError($"Internal Error: {ex.Message}");
        }
    }
    public async Task<Result<List<PointHistoryResDTO>>> GetPointHistoryAsync(string accountId)
    {
        try
        {
            var response = await _client.GetAsync($"accounts/{accountId}/history");

            if (response.IsSuccessStatusCode)
            {
                var history = await response.Content.ReadFromJsonAsync<List<PointHistoryResDTO>>();

                return history != null
                    ? Result<List<PointHistoryResDTO>>.Success(history)
                    : Result<List<PointHistoryResDTO>>.SystemError("No history records found.");
            }

            return Result<List<PointHistoryResDTO>>.SystemError($"Error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return Result<List<PointHistoryResDTO>>.SystemError($"Internal Error: {ex.Message}");
        }
    }
    public async Task<Result<ClaimRewardResDTO>> ClaimRewardAsync(ClaimRewardReqDTO request)
    {
        try
        {
            var response = await _client.PostAsJsonAsync("redemption/claim", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ClaimRewardResDTO>();
                return result != null
                    ? Result<ClaimRewardResDTO>.Success(result, "Redemption request created (Pending).")
                    : Result<ClaimRewardResDTO>.SystemError("Failed to process redemption response.");
            }


            var errorDetail = await response.Content.ReadAsStringAsync();
            return Result<ClaimRewardResDTO>.SystemError($"Redemption Failed: {errorDetail}");
        }
        catch (Exception ex)
        {
            return Result<ClaimRewardResDTO>.SystemError($"Internal Error: {ex.Message}");
        }
    }
    public async Task<Result<List<AvailableRewardResDTO>>> GetAvailableRewardsAsync()
    {
        try
        {
            var response = await _client.GetAsync($"rewards/active/YaungMel");

            if (response.IsSuccessStatusCode)
            {
                var rewards = await response.Content.ReadFromJsonAsync<List<AvailableRewardResDTO>>();

                return rewards != null
                    ? Result<List<AvailableRewardResDTO>>.Success(rewards, "Rewards retrieved successfully.")
                    : Result<List<AvailableRewardResDTO>>.SystemError("No rewards data found.");
            }

            return Result<List<AvailableRewardResDTO>>.SystemError($"Failed to fetch rewards: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return Result<List<AvailableRewardResDTO>>.SystemError($"Internal Error: {ex.Message}");
        }
    }

    #endregion

    public async Task<Result<List<PendingRedemptionResDTO>>> GetPendingRedemptionsAsync()
    {
        try
        {
            var response = await _client.GetAsync("admin/redemptions/pending");

            if (response.IsSuccessStatusCode)
            {
                var redemptions = await response.Content.ReadFromJsonAsync<List<PendingRedemptionResDTO>>();

                return redemptions != null
                    ? Result<List<PendingRedemptionResDTO>>.Success(redemptions)
                    : Result<List<PendingRedemptionResDTO>>.SystemError("No pending redemptions found.");
            }

            return Result<List<PendingRedemptionResDTO>>.SystemError($"Error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return Result<List<PendingRedemptionResDTO>>.SystemError($"Internal Error: {ex.Message}");
        }
    }
    public async Task<Result<bool>> UpdateRedemptionStatusAsync(string redemptionId, RedemptionStatus status)
    {
        try
        {
            var request = new UpdateRedemptionStatusReqDTO { Status = status };

            var response = await _client.PutAsJsonAsync($"admin/redemptions/{redemptionId}/status", request);

            if (response.IsSuccessStatusCode)
            {
                return Result<bool>.Success(true, $"Redemption status updated to {status}.");
            }

            var errorMsg = await response.Content.ReadAsStringAsync();
            return Result<bool>.SystemError($"Update Failed: {response.StatusCode} - {errorMsg}");
        }
        catch (Exception ex)
        {
            return Result<bool>.SystemError($"Internal Error: {ex.Message}");
        }
    }

    public async Task<Result<CreateRewardResDTO>> CreateRewardAsync(CreateRewardReqDTO request)
    {
        try
        {

            request.SystemId = "YaungMel";
            var response = await _client.PostAsJsonAsync("rewards", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CreateRewardResDTO>();

                return result != null
                    ? Result<CreateRewardResDTO>.Success(result, "Reward created successfully.")
                    : Result<CreateRewardResDTO>.SystemError("Failed to parse reward data.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return Result<CreateRewardResDTO>.SystemError($"Reward Creation Failed: {response.StatusCode} - {errorContent}");
        }
        catch (Exception ex)
        {
            return Result<CreateRewardResDTO>.SystemError($"Internal Error: {ex.Message}");
        }
    }
}
