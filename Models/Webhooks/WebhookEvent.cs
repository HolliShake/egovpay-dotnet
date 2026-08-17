using System.Text.Json;
using System.Text.Json.Serialization;

namespace EGovPay.Models.Webhooks;

/// <summary>
/// [PLACEHOLDER] Envelope eGovPay is assumed to POST to your configured
/// webhook URL, shaped after Stripe's Event object
/// (<c>{ "id", "type", "created", "data": { "object": {...} } }</c>).
/// Confirm real event `type` strings and payload shape once documented.
/// </summary>
public sealed class WebhookEvent
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// e.g. "payment_intent.succeeded", "payment_intent.failed", "refund.succeeded".
    /// [PLACEHOLDER] — purely illustrative naming convention.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("created")]
    public DateTimeOffset Created { get; set; }

    [JsonPropertyName("data")]
    public WebhookEventData Data { get; set; } = new();
}

public sealed class WebhookEventData
{
    /// <summary>Raw JSON of the affected object (a PaymentIntent, Refund, etc.) — deserialize with <see cref="GetObject{T}"/>.</summary>
    [JsonPropertyName("object")]
    public JsonElement Object { get; set; }

    public T GetObject<T>() => Object.Deserialize<T>()
        ?? throw new InvalidOperationException("Webhook event data.object could not be deserialized.");
}
