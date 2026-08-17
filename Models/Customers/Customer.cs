using System.Text.Json.Serialization;
using EGovPay.Models.Common;

namespace EGovPay.Models.Customers;

/// <summary>[PLACEHOLDER] A saved payer/citizen profile you can attach to future PaymentIntents.</summary>
public sealed class Customer : EGovPayEntity
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("mobile")]
    public string? Mobile { get; set; }

    /// <summary>
    /// If your agency links payments to eGov's PhilSys-backed identity
    /// (eVerify), this may correspond to the citizen's `uniqid` from the SSO
    /// profile payload. Unconfirmed for eGovPay specifically.
    /// </summary>
    [JsonPropertyName("eGov_uniqid")]
    public string? EGovUniqueId { get; set; }
}

/// <summary>[PLACEHOLDER] Request body for creating/updating a customer.</summary>
public sealed class CustomerCreateOptions
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("mobile")]
    public string? Mobile { get; init; }

    [JsonPropertyName("eGov_uniqid")]
    public string? EGovUniqueId { get; init; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; init; }
}
