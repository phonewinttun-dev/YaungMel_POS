using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using POSSampleOWN.domain.DTOs;
using POSSampleOWN.Responses;
using POSSampleOWN.shared;

namespace POSSampleOWN.domain.Features.Point;

public class PointService : IPointService
{
    private readonly HttpClient _client;

    public PointService(HttpClient clitent)
    {
        _client = clitent;
    }

    #region Account
    public async Task<ApiResponse<CreateAccountResDTO>> CreateAccountAsync(CreateAccountReqDTO request)
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
                    ? ApiResponse<CreateAccountResDTO>.Success(result, "Account created successfully.")
                    : ApiResponse<CreateAccountResDTO>.Fail("Failed to parse created account data.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return ApiResponse<CreateAccountResDTO>.Fail($"Creation Failed: {response.StatusCode} - {errorContent}");
        }
        catch (Exception ex)
        {
            return ApiResponse<CreateAccountResDTO>.Fail($"Internal Error: {ex.Message}");
        }
    }
    public async Task<ApiResponse<AccountListResponseWrapper>> GetAccountsAsync(AccountListReqDTO request)
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
                    ? ApiResponse<AccountListResponseWrapper>.Success(result)
                    : ApiResponse<AccountListResponseWrapper>.Fail("No account data found.");
            }

            return ApiResponse<AccountListResponseWrapper>.Fail($"Error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return ApiResponse<AccountListResponseWrapper>.Fail($"Internal Error: {ex.Message}");
        }
    }
    public async Task<ApiResponse<AccountLookupResponse>> LookupAccountAsync(string  userId)
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
                    ? ApiResponse<AccountLookupResponse>.Success(result)
                    : ApiResponse<AccountLookupResponse>.Fail("Account detail not found.");
            }

            return ApiResponse<AccountLookupResponse>.Fail($"Error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return ApiResponse<AccountLookupResponse>.Fail($"Internal Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CheckBalanceResDTO>> GetUserBalanceAsync(CheckBalanceReqDTO request)
    {
        try
        {

            string url = $"accounts/lookup/{request.SystemId}/{request.ExternalUserId}";

            var response = await _client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<CheckBalanceResDTO>();
                return data != null
                    ? ApiResponse<CheckBalanceResDTO>.Success(data)
                    : ApiResponse<CheckBalanceResDTO>.Fail("Data not found.");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return ApiResponse<CheckBalanceResDTO>.Fail("User not found in the system.");
            }

            return ApiResponse<CheckBalanceResDTO>.Fail($"Error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return ApiResponse<CheckBalanceResDTO>.Fail($"Internal Error: {ex.Message}");
        }
    }
    #endregion

    #region Earn and Use
    public async Task<ApiResponse<EarnPointResDTO>> EarnPointsAsync(EarnPointReqDTO request)
    {
        try
        {
            var response = await _client.PostAsJsonAsync("events/process", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<EarnPointResDTO>();

                return result != null
                    ? ApiResponse<EarnPointResDTO>.Success(result, "Points earned successfully.")
                    : ApiResponse<EarnPointResDTO>.Fail("Failed to parse response data.");
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            return ApiResponse<EarnPointResDTO>.Fail($"API Error: {response.StatusCode} - {errorMessage}");
        }
        catch (Exception ex)
        {
            return ApiResponse<EarnPointResDTO>.Fail($"Internal Error: {ex.Message}");
        }
    }
    public async Task<ApiResponse<List<PointHistoryResDTO>>> GetPointHistoryAsync(string accountId)
    {
        try
        {
            var response = await _client.GetAsync($"accounts/{accountId}/history");

            if (response.IsSuccessStatusCode)
            {
                var history = await response.Content.ReadFromJsonAsync<List<PointHistoryResDTO>>();

                return history != null
                    ? ApiResponse<List<PointHistoryResDTO>>.Success(history)
                    : ApiResponse<List<PointHistoryResDTO>>.Fail("No history records found.");
            }

            return ApiResponse<List<PointHistoryResDTO>>.Fail($"Error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PointHistoryResDTO>>.Fail($"Internal Error: {ex.Message}");
        }
    }
    public async Task<ApiResponse<ClaimRewardResDTO>> ClaimRewardAsync(ClaimRewardReqDTO request)
    {
        try
        {
            var response = await _client.PostAsJsonAsync("redemption/claim", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ClaimRewardResDTO>();
                return result != null
                    ? ApiResponse<ClaimRewardResDTO>.Success(result, "Redemption request created (Pending).")
                    : ApiResponse<ClaimRewardResDTO>.Fail("Failed to process redemption response.");
            }


            var errorDetail = await response.Content.ReadAsStringAsync();
            return ApiResponse<ClaimRewardResDTO>.Fail($"Redemption Failed: {errorDetail}");
        }
        catch (Exception ex)
        {
            return ApiResponse<ClaimRewardResDTO>.Fail($"Internal Error: {ex.Message}");
        }
    }
    public async Task<ApiResponse<List<AvailableRewardResDTO>>> GetAvailableRewardsAsync()
    {
        try
        {
            var response = await _client.GetAsync($"rewards/active/YaungMel");

            if (response.IsSuccessStatusCode)
            {
                var rewards = await response.Content.ReadFromJsonAsync<List<AvailableRewardResDTO>>();

                return rewards != null
                    ? ApiResponse<List<AvailableRewardResDTO>>.Success(rewards, "Rewards retrieved successfully.")
                    : ApiResponse<List<AvailableRewardResDTO>>.Fail("No rewards data found.");
            }

            return ApiResponse<List<AvailableRewardResDTO>>.Fail($"Failed to fetch rewards: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<AvailableRewardResDTO>>.Fail($"Internal Error: {ex.Message}");
        }
    }

    #endregion

    public async Task<ApiResponse<List<PendingRedemptionResDTO>>> GetPendingRedemptionsAsync()
    {
        try
        {
            var response = await _client.GetAsync("admin/redemptions/pending");

            if (response.IsSuccessStatusCode)
            {
                var redemptions = await response.Content.ReadFromJsonAsync<List<PendingRedemptionResDTO>>();

                return redemptions != null
                    ? ApiResponse<List<PendingRedemptionResDTO>>.Success(redemptions)
                    : ApiResponse<List<PendingRedemptionResDTO>>.Fail("No pending redemptions found.");
            }

            return ApiResponse<List<PendingRedemptionResDTO>>.Fail($"Error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PendingRedemptionResDTO>>.Fail($"Internal Error: {ex.Message}");
        }
    }
    public async Task<ApiResponse<bool>> UpdateRedemptionStatusAsync(string redemptionId, RedemptionStatus status)
    {
        try
        {
            var request = new UpdateRedemptionStatusReqDTO { Status = status };

            var response = await _client.PutAsJsonAsync($"admin/redemptions/{redemptionId}/status", request);

            if (response.IsSuccessStatusCode)
            {
                return ApiResponse<bool>.Success(true, $"Redemption status updated to {status}.");
            }

            var errorMsg = await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Fail($"Update Failed: {response.StatusCode} - {errorMsg}");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Fail($"Internal Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CreateRewardResDTO>> CreateRewardAsync(CreateRewardReqDTO request)
    {
        try
        {

            request.SystemId = "YaungMel";
            var response = await _client.PostAsJsonAsync("rewards", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CreateRewardResDTO>();

                return result != null
                    ? ApiResponse<CreateRewardResDTO>.Success(result, "Reward created successfully.")
                    : ApiResponse<CreateRewardResDTO>.Fail("Failed to parse reward data.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return ApiResponse<CreateRewardResDTO>.Fail($"Reward Creation Failed: {response.StatusCode} - {errorContent}");
        }
        catch (Exception ex)
        {
            return ApiResponse<CreateRewardResDTO>.Fail($"Internal Error: {ex.Message}");
        }
    }
}
