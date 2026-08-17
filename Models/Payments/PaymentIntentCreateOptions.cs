using System.Text.Json.Serialization;

namespace EGovPay.Models.Payments;

/// <summary>[PLACEHOLDER] Request body for creating a PaymentIntent. Verify required vs. optional fields against the real API.</summary>
public sealed class PaymentIntentCreateOptions
{
    /// <summary>Required. Amount in centavos (e.g. 150000 = ₱1,500.00).</summary>
    [JsonPropertyName("amount")]
    public required long Amount { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; } = "PHP";

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("reference_number")]
    public string? ReferenceNumber { get; init; }

    [JsonPropertyName("agency_code")]
    public string? AgencyCode { get; init; }

    /// <summary>
    /// Restrict which channel(s) the payer can use. Omit to let eGovPay auto-route
    /// through "the most optimal channel," per its marketing copy.
    /// </summary>
    [JsonPropertyName("allowed_payment_channels")]
    public List<PaymentChannel>? AllowedPaymentChannels { get; init; }

    [JsonPropertyName("payer")]
    public PaymentIntentPayer? Payer { get; init; }

    /// <summary>Where to send the payer back to after completing/cancelling payment.</summary>
    [JsonPropertyName("return_url")]
    public string? ReturnUrl { get; init; }

    /// <summary>Per-integration webhook override; otherwise the account-level default configured in the portal is used.</summary>
    [JsonPropertyName("webhook_url")]
    public string? WebhookUrl { get; init; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; init; }
}

/// <summary>[PLACEHOLDER] Request body for confirming a PaymentIntent with a chosen channel.</summary>
public sealed class PaymentIntentConfirmOptions
{
    [JsonPropertyName("payment_channel")]
    public PaymentChannel? PaymentChannel { get; init; }

    [JsonPropertyName("return_url")]
    public string? ReturnUrl { get; init; }
}
