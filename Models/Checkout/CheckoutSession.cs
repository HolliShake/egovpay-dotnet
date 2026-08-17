using System.Text.Json.Serialization;
using EGovPay.Models.Common;
using EGovPay.Models.Payments;

namespace EGovPay.Models.Checkout;

/// <summary>
/// [PLACEHOLDER] A hosted, redirect-based checkout flow — the "give me a URL
/// I can send my citizen/payer to" alternative to driving a PaymentIntent
/// yourself. Modeled after Stripe Checkout Sessions.
/// </summary>
public sealed class CheckoutSession : EGovPayEntity
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty; // e.g. "open" | "complete" | "expired" — TODO confirm values

    [JsonPropertyName("payment_intent_id")]
    public string? PaymentIntentId { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("success_url")]
    public string SuccessUrl { get; set; } = string.Empty;

    [JsonPropertyName("cancel_url")]
    public string CancelUrl { get; set; } = string.Empty;

    [JsonPropertyName("amount_total")]
    public long AmountTotal { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "PHP";

    [JsonPropertyName("expires_at")]
    public DateTimeOffset? ExpiresAt { get; set; }
}
