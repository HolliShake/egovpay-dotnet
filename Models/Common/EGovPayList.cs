using System.Text.Json.Serialization;

namespace EGovPay.Models.Common;

/// <summary>
/// [PLACEHOLDER] Generic paged-list envelope, shaped after Stripe's
/// <c>{ "object": "list", "data": [...], "has_more": bool }</c> convention.
/// Adjust property names once the real pagination shape is confirmed — some
/// gov.ph APIs use "page"/"total"/"per_page" style pagination instead.
/// </summary>
public sealed class EGovPayList<T>
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = "list";

    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = new();

    [JsonPropertyName("has_more")]
    public bool HasMore { get; set; }

    [JsonPropertyName("total_count")]
    public int? TotalCount { get; set; }
}
