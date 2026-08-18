using System.Text.Json.Serialization;

namespace EGovPay.Models.Transactions;

/// <summary>A line item included in a transaction.</summary>
public sealed class TransactionItem
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    // Docs list this field as "Double", but every reference payload (cURL/C#) sends it as a quoted string.
    [JsonPropertyName("amount")]
    public required double Amount { get; init; }
}
