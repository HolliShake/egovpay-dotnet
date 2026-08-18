namespace EGovPay.Models.Transactions;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public class TransactionDetailResponse
{
    [JsonPropertyName("data")]
    public TransactionData Data { get; set; }
}

public class TransactionData
{
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; }

    [JsonPropertyName("refno")]
    public string RefNo { get; set; }

    [JsonPropertyName("txnid")]
    public string TxnId { get; set; }

    [JsonPropertyName("environment_type")]
    public string EnvironmentType { get; set; }

    [JsonPropertyName("items")]
    public List<TransactionItem> Items { get; set; }

    [JsonPropertyName("amount")]
    [JsonConverter(typeof(StringDecimalConverter))]
    public decimal Amount { get; set; }

    [JsonPropertyName("system_fee")]
    [JsonConverter(typeof(StringDecimalConverter))]
    public decimal SystemFee { get; set; }

    [JsonPropertyName("channel_fee")]
    [JsonConverter(typeof(StringDecimalConverter))]
    public decimal ChannelFee { get; set; }

    [JsonPropertyName("partner_fee")]
    [JsonConverter(typeof(StringDecimalConverter))]
    public decimal PartnerFee { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    [JsonPropertyName("payment_status")]
    public string PaymentStatus { get; set; }

    [JsonPropertyName("payment_channel")]
    public string PaymentChannel { get; set; }

    [JsonPropertyName("payment_channel_uuid")]
    public string PaymentChannelUuid { get; set; }

    [JsonPropertyName("payment_channel_branch")]
    public string PaymentChannelBranch { get; set; }

    [JsonPropertyName("callback_url")]
    public string CallbackUrl { get; set; }

    [JsonPropertyName("redirect_url")]
    public string RedirectUrl { get; set; }

    [JsonPropertyName("paid_at")]
    [JsonConverter(typeof(EGovPayDateTimeConverter))]
    public DateTime? PaidAt { get; set; }

    [JsonPropertyName("link_expires_at")]
    [JsonConverter(typeof(EGovPayDateTimeConverter))]
    public DateTime? LinkExpiresAt { get; set; }

    [JsonPropertyName("expires_at")]
    [JsonConverter(typeof(EGovPayDateTimeConverter))]
    public DateTime? ExpiresAt { get; set; }

    [JsonPropertyName("created_at")]
    [JsonConverter(typeof(EGovPayDateTimeConverter))]
    public DateTime? CreatedAt { get; set; }
}

/// <summary>
/// eGovPay returns monetary values as quoted strings, e.g. "1000.0000".
/// Deserializes/serializes them as decimal while preserving round-trip through JSON strings.
/// </summary>
public class StringDecimalConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s)) return 0m;
            return decimal.Parse(s, NumberStyles.Number, CultureInfo.InvariantCulture);
        }

        // Fallback in case the API ever sends a raw number
        return reader.GetDecimal();
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
    }
}

/// <summary>
/// eGovPay returns dates like "August 8, 2025 11:59:59 PM". Handles null for fields
/// like paid_at that may be absent until payment completes.
/// </summary>
public class EGovPayDateTimeConverter : JsonConverter<DateTime?>
{
    private const string Format = "MMMM d, yyyy h:mm:ss tt";

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;

        var s = reader.GetString();
        if (string.IsNullOrWhiteSpace(s)) return null;

        if (DateTime.TryParseExact(s, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            return result;

        // Fallback for any slight format drift (e.g. single-digit vs padded values)
        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fallback))
            return fallback;

        throw new JsonException($"Unable to parse eGovPay date: '{s}'");
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value.Value.ToString(Format, CultureInfo.InvariantCulture));
        else
            writer.WriteNullValue();
    }
}