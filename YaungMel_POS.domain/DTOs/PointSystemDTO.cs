using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YaungMel_POS.shared;

namespace YaungMel_POS.domain.DTOs;

public class AccountItemDTO
{
    public string Id { get; set; }
    public string SystemId { get; set; }
    public string ExternalUserId { get; set; }
    public int CurrentBalance { get; set; }
    public int LifetimePoints { get; set; }
    public string Tier { get; set; }
    public string Mobile { get; set; }
    public string Email { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class EarnPointReqDTO
{
    [JsonPropertyName("externalUserId")]
    public string ExternalUserId { get; set; }

    [JsonPropertyName("eventKey")]
    public string EventKey { get; set; }

    [JsonPropertyName("eventValue")]
    public decimal EventValue { get; set; }

    [JsonPropertyName("referenceId")]
    public string ReferenceId { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("mobile")]
    public string Mobile { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }
}

public class EarnPointResDTO
{
    public int CurrentBalance { get; set; }
}

public class CheckBalanceReqDTO
{
    public string SystemId { get; set; }
    public string ExternalUserId { get; set; }
}

public class CheckBalanceResDTO
{
    public string AccountId { get; set; }
    public string ExternalUserId { get; set; }
    public int CurrentBalance { get; set; }
    public int LifetimePoints { get; set; }
    public string Tier { get; set; }
}

public class AvailableRewardResDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int PointCost { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }

}

public class ClaimRewardResDTO
{
    public string RedemptionId { get; set; }
    public string Status { get; set; }
    public int RemainingBalance { get; set; }
}

public class ClaimRewardReqDTO
{
    [JsonPropertyName("externalUserId")]
    public string ExternalUserId { get; set; }

    [JsonPropertyName("rewardId")]
    public string RewardId { get; set; }

    [JsonPropertyName("notes")]
    public string Notes { get; set; }
}

public class CreateAccountReqDTO
{
    //[JsonPropertyName("systemId")]
    //public string SystemId { get; set; } = "YaungMel";

    [JsonPropertyName("tier")]
    public CustomerTier Tier { get; set; }

    [JsonPropertyName("mobile")]
    public string Mobile { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }
}

public class CreateAccount
{
    [JsonPropertyName("systemId")]
    public string SystemId { get; set; }

    [JsonPropertyName("externalUserId")]
    public string ExternalUserId { get; set; }

    [JsonPropertyName("tier")]
    public CustomerTier Tier { get; set; }

    [JsonPropertyName("mobile")]
    public string Mobile { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }
}
public class CreateAccountResDTO
{
    public string Id { get; set; }
    public string SystemId { get; set; }
    public string ExternalUserId { get; set; }
    public int CurrentBalance { get; set; }
    public int LifetimePoints { get; set; }
    public string Tier { get; set; }
    public string Mobile { get; set; }
    public string Email { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AccountListReqDTO
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SystemId { get; set; } = "YaungMel";
    public CustomerTier? Tier { get; set; } 
    public string? SearchTerm { get; set; }
}

public class AccountListResponseWrapper
{
    public List<AccountItemDTO> Items { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class PointHistoryResDTO
{
    public int PointDelta { get; set; } 
    public string EventKey { get; set; }
    public string ReferenceId { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PendingRedemptionResDTO
{
    public string Id { get; set; }
    public string SystemId { get; set; }
    public string ExternalUserId { get; set; }
    public string RewardName { get; set; }
    public string Status { get; set; }
    public int PointCost { get; set; }
    public DateTime RedeemedAt { get; set; }
}

public class UpdateRedemptionStatusReqDTO
{
    [JsonPropertyName("status")]
    public RedemptionStatus Status { get; set; }
}

public class AccountLookupResponse
{
    public Guid AccountId { get; set; }
    public string ExternalUserId { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public int LifetimePoints { get; set; }
    public string Tier { get; set; } = string.Empty;
}

public class CreateRewardReqDTO
{
    public string SystemId { get; set; } = "YaungMel";
    public string Name { get; set; }
    public string Description { get; set; }
    public int PointCost { get; set; }
    public int StockQuantity { get; set; }
}

public class CreateRewardResDTO
{
    public string Id { get; set; }
    public string SystemId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int PointCost { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
}