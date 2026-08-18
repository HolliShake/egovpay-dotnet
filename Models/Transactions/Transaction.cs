using System.Text.Json;
using System.Text.Json.Serialization;

namespace EGovPay.Models.Transactions;

/// <summary>
/// A transaction, as returned by the Generate Payment, Check Transaction Details, and
/// Void Transaction endpoints. The docs don't publish a full response schema, so this
/// mirrors the fields that are echoed back from the request plus a "uuid" identifier
/// (used in the documented <c>/api/v1/transaction/{{uuid}}</c> routes). Anything the API
/// returns that isn't modeled here is preserved in <see cref="ExtensionData"/>.
/// </summary>
public sealed class Transaction
{
    [JsonPropertyName("uuid")]
    public string? Uuid { get; set; }

    [JsonPropertyName("txnid")]
    public string? TxnId { get; set; }

    [JsonPropertyName("amount")]
    public string? Amount { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    /// <summary>Not documented as an enum anywhere on the public reference — left as a raw string.</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("settlement_template_uuid")]
    public string? SettlementTemplateUuid { get; set; }

    [JsonPropertyName("redirect_url")]
    public string? RedirectUrl { get; set; }

    [JsonPropertyName("callback_url")]
    public string? CallbackUrl { get; set; }

    [JsonPropertyName("mobile")]
    public string? Mobile { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("expires_at")]
    public string? ExpiresAt { get; set; }

    [JsonPropertyName("link_expires_at")]
    public string? LinkExpiresAt { get; set; }

    [JsonPropertyName("items")]
    public List<TransactionItem>? Items { get; set; }

    /// <summary>Not in the docs' Body Parameters table, but the sample section is titled "Generate Payment Gateway Link" — likely the payer-facing redirect URL.</summary>
    [JsonPropertyName("link")]
    public string? Link { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
