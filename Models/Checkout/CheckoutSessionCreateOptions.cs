using System.Text.Json.Serialization;
using EGovPay.Models.Payments;

namespace EGovPay.Models.Checkout;

/// <summary>[PLACEHOLDER] Request body for creating a hosted Checkout Session.</summary>
public sealed class CheckoutSessionCreateOptions
{
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

    [JsonPropertyName("allowed_payment_channels")]
    public List<PaymentChannel>? AllowedPaymentChannels { get; init; }

    [JsonPropertyName("success_url")]
    public required string SuccessUrl { get; init; }

    [JsonPropertyName("cancel_url")]
    public required string CancelUrl { get; init; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; init; }
}
