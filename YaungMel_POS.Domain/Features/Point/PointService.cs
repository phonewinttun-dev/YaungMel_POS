using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Shared;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YaungMel_POS.Domain.Features.Point;

public class PointService : IPointService
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public PointService(HttpClient clitent)
    {
        _client = clitent;
    }

    private async Task<T?> ReadResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content)) return default;

        try
        {
            // 1. Try to parse as the raw type first
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }
        catch
        {
            try
            {
                // 2. Try as Result<T> wrapper
                var wrapped = JsonSerializer.Deserialize<ResultWrapper<T>>(content, _jsonOptions);
                if (wrapped != null && wrapped.IsSuccess && wrapped.Data != null)
                {
                    return wrapped.Data;
                }
            }
            catch { }

            // 3. If T is a List<X>, it might be an object like { "items": [...] } or { "data": [...] }
            if (typeof(T).IsGenericType && (typeof(T).GetGenericTypeDefinition() == typeof(List<>) || typeof(T).GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                try
                {
                    using var doc = JsonDocument.Parse(content);
                    var root = doc.RootElement;
                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        if (root.TryGetProperty("items", out var items))
                        {
                            return items.Deserialize<T>(_jsonOptions);
                        }
                        if (root.TryGetProperty("data", out var data))
                        {
                            return data.Deserialize<T>(_jsonOptions);
                        }
                    }
                }
                catch { }
            }

            return default;
        }
    }

    // Helper class for parsing potential Result wrappers
    private class ResultWrapper<T>
    {
        public bool IsSuccess { get; set; }
        public bool Success { get; set; } // Some APIs use "success" instead of "isSuccess"
        public T? Data { get; set; }
    }

    #region Account
    public async Task<PagedResult<CreateAccountResDTO>> CreateAccountAsync(CreateAccountReqDTO request)
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
                var result = await ReadResponseAsync<CreateAccountResDTO>(response);

                return result != null
                    ? PagedResult<CreateAccountResDTO>.Success(result, "Account created successfully.")
                    : PagedResult<CreateAccountResDTO>.SystemError("Failed to parse created account data.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return PagedResult<CreateAccountResDTO>.SystemError($"Creation Failed: {response.StatusCode} - {errorContent}");
        }
        catch (Exception ex)
        {
            return PagedResult<CreateAccountResDTO>.SystemError($"Internal Error: {ex.Message}");
        }
    }
    public async Task<PagedResult<AccountListResponseWrapper>> GetAccountsAsync(AccountListReqDTO request)
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
                var result = await ReadResponseAsync<AccountListResponseWrapper>(response);
                return result != null
                    ? PagedResult<AccountListResponseWrapper>.Success(result)
                    : PagedResult<AccountListResponseWrapper>.SystemError("No account data found.");
            }

            return PagedResult<AccountListResponseWrapper>.SystemError($"Error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return PagedResult<AccountListResponseWrapper>.SystemError($"Internal Error: {ex.Message}");
        }
    }
    public async Task<PagedResult<AccountLookupResponse>> LookupAccountAsync(string  userId)
    {
        try
        {
            var url = $"accounts/lookup/{userId}";

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await _client.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                var result = await ReadResponseAsync<AccountLookupResponse>(response);
                return result != null
                    ? PagedResult<AccountLookupResponse>.Success(result)
                    : PagedResult<AccountLookupResponse>.SystemError("Account detail not found.");
            }

            return PagedResult<AccountLookupResponse>.SystemError($"Error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return PagedResult<AccountLookupResponse>.SystemError($"Internal Error: {ex.Message}");
        }
    }

    public async Task<PagedResult<CheckBalanceResDTO>> GetUserBalanceAsync(CheckBalanceReqDTO request)
    {
        try
        {

            string url = $"accounts/lookup/{request.SystemId}/{request.ExternalUserId}";

            var response = await _client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await ReadResponseAsync<CheckBalanceResDTO>(response);
                return data != null
                    ? PagedResult<CheckBalanceResDTO>.Success(data)
                    : PagedResult<CheckBalanceResDTO>.SystemError("Data not found.");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return PagedResult<CheckBalanceResDTO>.SystemError("User not found in the system.");
            }

            return PagedResult<CheckBalanceResDTO>.SystemError($"Error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return PagedResult<CheckBalanceResDTO>.SystemError($"Internal Error: {ex.Message}");
        }
    }
    #endregion

    #region Earn and Use
    public async Task<PagedResult<EarnPointResDTO>> EarnPointsAsync(EarnPointReqDTO request)
    {
        try
        {
            var response = await _client.PostAsJsonAsync("events/process", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await ReadResponseAsync<EarnPointResDTO>(response);

                return result != null
                    ? PagedResult<EarnPointResDTO>.Success(result, "Points earned successfully.")
                    : PagedResult<EarnPointResDTO>.SystemError("Failed to parse response data.");
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            return PagedResult<EarnPointResDTO>.SystemError($"API Error: {response.StatusCode} - {errorMessage}");
        }
        catch (Exception ex)
        {
            return PagedResult<EarnPointResDTO>.SystemError($"Internal Error: {ex.Message}");
        }
    }
    public async Task<PagedResult<List<PointHistoryResDTO>>> GetPointHistoryAsync(string accountId)
    {
        try
        {
            var response = await _client.GetAsync($"accounts/{accountId}/history");

            if (response.IsSuccessStatusCode)
            {
                var history = await ReadResponseAsync<List<PointHistoryResDTO>>(response);

                return history != null
                    ? PagedResult<List<PointHistoryResDTO>>.Success(history)
                    : PagedResult<List<PointHistoryResDTO>>.SystemError("No history records found.");
            }

            return PagedResult<List<PointHistoryResDTO>>.SystemError($"Error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return PagedResult<List<PointHistoryResDTO>>.SystemError($"Internal Error: {ex.Message}");
        }
    }
    public async Task<PagedResult<ClaimRewardResDTO>> ClaimRewardAsync(ClaimRewardReqDTO request)
    {
        try
        {
            var response = await _client.PostAsJsonAsync("redemption/claim", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await ReadResponseAsync<ClaimRewardResDTO>(response);
                if (result == null)
                {
                    result = new ClaimRewardResDTO { Status = "Pending" };
                }

                // If remaining balance is not returned by the claim API, fetch it manually
                if (result.RemainingBalance == null)
                {
                    var balanceResult = await GetUserBalanceAsync(new CheckBalanceReqDTO
                    {
                        SystemId = "YaungMel",
                        ExternalUserId = request.ExternalUserId
                    });

                    if (balanceResult.IsSuccess && balanceResult.Data != null)
                    {
                        result.RemainingBalance = balanceResult.Data.CurrentBalance;
                    }
                }

                return PagedResult<ClaimRewardResDTO>.Success(result, "Redemption request created.");
            }


            var errorDetail = await response.Content.ReadAsStringAsync();
            return PagedResult<ClaimRewardResDTO>.SystemError($"Redemption Failed: {errorDetail}");
        }
        catch (Exception ex)
        {
            return PagedResult<ClaimRewardResDTO>.SystemError($"Internal Error: {ex.Message}");
        }
    }
    public async Task<PagedResult<List<AvailableRewardResDTO>>> GetAvailableRewardsAsync()
    {
        try
        {
            var response = await _client.GetAsync("rewards/active");

            if (response.IsSuccessStatusCode)
            {
                var rewards = await ReadResponseAsync<List<AvailableRewardResDTO>>(response);

                return rewards != null
                    ? PagedResult<List<AvailableRewardResDTO>>.Success(rewards, "Rewards retrieved successfully.")
                    : PagedResult<List<AvailableRewardResDTO>>.SystemError("No rewards data found.");
            }

            return PagedResult<List<AvailableRewardResDTO>>.SystemError($"Failed to fetch rewards: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return PagedResult<List<AvailableRewardResDTO>>.SystemError($"Internal Error: {ex.Message}");
        }
    }

    #endregion

    public async Task<PagedResult<List<PendingRedemptionResDTO>>> GetPendingRedemptionsAsync()
    {
        try
        {
            var response = await _client.GetAsync("admin/redemptions/pending");

            if (response.IsSuccessStatusCode)
            {
                var redemptions = await ReadResponseAsync<List<PendingRedemptionResDTO>>(response);

                return redemptions != null
                    ? PagedResult<List<PendingRedemptionResDTO>>.Success(redemptions)
                    : PagedResult<List<PendingRedemptionResDTO>>.SystemError("No pending redemptions found.");
            }

            return PagedResult<List<PendingRedemptionResDTO>>.SystemError($"Error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return PagedResult<List<PendingRedemptionResDTO>>.SystemError($"Internal Error: {ex.Message}");
        }
    }
    public async Task<PagedResult<bool>> UpdateRedemptionStatusAsync(string redemptionId, RedemptionStatus status)
    {
        try
        {
            var request = new UpdateRedemptionStatusReqDTO { Status = status };

            var response = await _client.PutAsJsonAsync($"admin/redemptions/{redemptionId}/status", request);

            if (response.IsSuccessStatusCode)
            {
                return PagedResult<bool>.Success(true, $"Redemption status updated to {status}.");
            }

            var errorMsg = await response.Content.ReadAsStringAsync();
            return PagedResult<bool>.SystemError($"Update Failed: {response.StatusCode} - {errorMsg}");
        }
        catch (Exception ex)
        {
            return PagedResult<bool>.SystemError($"Internal Error: {ex.Message}");
        }
    }

    public async Task<PagedResult<CreateRewardResDTO>> CreateRewardAsync(CreateRewardReqDTO request)
    {
        try
        {

            request.SystemId = "YaungMel";
            var response = await _client.PostAsJsonAsync("rewards", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await ReadResponseAsync<CreateRewardResDTO>(response);

                return result != null
                    ? PagedResult<CreateRewardResDTO>.Success(result, "Reward created successfully.")
                    : PagedResult<CreateRewardResDTO>.SystemError("Failed to parse reward data.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return PagedResult<CreateRewardResDTO>.SystemError($"Reward Creation Failed: {response.StatusCode} - {errorContent}");
        }
        catch (Exception ex)
        {
            return PagedResult<CreateRewardResDTO>.SystemError($"Internal Error: {ex.Message}");
        }
    }
    public async Task<PagedResult<AvailableRewardResDTO>> UpdateRewardAsync(string id, UpdateRewardReqDTO request)
    {
        try
        {
            var response = await _client.PutAsJsonAsync($"rewards/{id}", request);

            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return PagedResult<AvailableRewardResDTO>.Success(new AvailableRewardResDTO(), "Reward updated successfully.");
                }

                var result = await ReadResponseAsync<AvailableRewardResDTO>(response);
                return result != null
                    ? PagedResult<AvailableRewardResDTO>.Success(result, "Reward updated successfully.")
                    : PagedResult<AvailableRewardResDTO>.SystemError("Failed to parse reward data.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return PagedResult<AvailableRewardResDTO>.SystemError($"Reward Update Failed: {response.StatusCode} - {errorContent}");
        }
        catch (Exception ex)
        {
            return PagedResult<AvailableRewardResDTO>.SystemError($"Internal Error: {ex.Message}");
        }
    }

    public async Task<PagedResult<bool>> DeleteRewardAsync(string id)
    {
        try
        {
            var response = await _client.DeleteAsync($"rewards/{id}");

            if (response.IsSuccessStatusCode)
            {
                return PagedResult<bool>.Success(true, "Reward deleted successfully.");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return PagedResult<bool>.SystemError($"Reward Deletion Failed: {response.StatusCode} - {errorContent}");
        }
        catch (Exception ex)
        {
            return PagedResult<bool>.SystemError($"Internal Error: {ex.Message}");
        }
    }
}
