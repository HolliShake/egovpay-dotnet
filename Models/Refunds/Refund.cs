using System.Text.Json.Serialization;
using EGovPay.Models.Common;

namespace EGovPay.Models.Refunds;

/// <summary>[PLACEHOLDER] Status of a refund. Confirm real values against the API docs.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RefundStatus
{
    Pending,
    Succeeded,
    Failed,
    Canceled,
}

/// <summary>[PLACEHOLDER] A refund issued against a previously succeeded PaymentIntent.</summary>
public sealed class Refund : EGovPayEntity
{
    [JsonPropertyName("payment_intent_id")]
    public string PaymentIntentId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "PHP";

    [JsonPropertyName("status")]
    public RefundStatus Status { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}

/// <summary>[PLACEHOLDER] Request body for creating a refund.</summary>
public sealed class RefundCreateOptions
{
    [JsonPropertyName("payment_intent_id")]
    public required string PaymentIntentId { get; init; }

    /// <summary>Omit for a full refund; set for a partial refund (in centavos).</summary>
    [JsonPropertyName("amount")]
    public long? Amount { get; init; }

    [JsonPropertyName("reason")]
    public string? Reason { get; init; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; init; }
}
