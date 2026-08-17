using System.Text.Json.Serialization;

namespace EGovPay.Models.Common;

/// <summary>
/// Common fields expected to be present on most eGovPay API objects, modeled
/// after the conventions Stripe (and most REST payment APIs) use. Field names
/// are [PLACEHOLDER] pending the real schema — adjust the [JsonPropertyName]
/// attributes here (and on subclasses) once you have the actual response shape.
/// </summary>
public abstract class EGovPayEntity
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Discriminator field, e.g. "payment_intent", "refund", "customer".</summary>
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("created")]
    public DateTimeOffset? Created { get; set; }

    /// <summary>
    /// Arbitrary caller-supplied key/value data. Common on Stripe-like APIs for
    /// attaching your own reference ids (e.g. internal transaction/OR number).
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Catches any fields the API returns that this SDK doesn't yet model
    /// explicitly, so nothing is silently dropped while the schema is unconfirmed.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, System.Text.Json.JsonElement>? ExtensionData { get; set; }
}
