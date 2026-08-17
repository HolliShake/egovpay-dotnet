using System.Text.Json.Serialization;
using EGovPay.Models.Common;

namespace EGovPay.Models.Payments;

/// <summary>
/// [PLACEHOLDER] A PaymentIntent tracks a single payment attempt/collection
/// through its lifecycle, mirroring Stripe's PaymentIntent object. This is the
/// central resource most eGovPay integrations (fee collection, permit
/// payments, etc.) would build around — but its exact field names are not
/// publicly confirmed. Cross-check every property below against the real
/// eGovPay API reference once you have portal access.
/// </summary>
public sealed class PaymentIntent : EGovPayEntity
{
    /// <summary>Amount in the smallest currency unit (centavos for PHP), matching Stripe's convention.</summary>
    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("amount_received")]
    public long? AmountReceived { get; set; }

    /// <summary>ISO 4217 currency code. Almost certainly always "PHP" for a gov.ph gateway.</summary>
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "PHP";

    [JsonPropertyName("status")]
    public PaymentIntentStatus Status { get; set; }

    /// <summary>Free-text description shown to the payer / on statements (e.g. "Business Permit Renewal - BR-2026-00123").</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Your own reference number for this collection (OR number, application id, etc.).</summary>
    [JsonPropertyName("reference_number")]
    public string? ReferenceNumber { get; set; }

    /// <summary>The agency/merchant account this payment is being collected on behalf of.</summary>
    [JsonPropertyName("agency_code")]
    public string? AgencyCode { get; set; }

    [JsonPropertyName("payment_channel")]
    public PaymentChannel? PaymentChannel { get; set; }

    /// <summary>Hosted checkout / payment-instruction URL to redirect the payer to, once available.</summary>
    [JsonPropertyName("checkout_url")]
    public string? CheckoutUrl { get; set; }

    [JsonPropertyName("client_secret")]
    public string? ClientSecret { get; set; }

    [JsonPropertyName("payer")]
    public PaymentIntentPayer? Payer { get; set; }

    [JsonPropertyName("last_payment_error")]
    public PaymentIntentError? LastPaymentError { get; set; }
}

public sealed class PaymentIntentPayer
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("mobile")]
    public string? Mobile { get; set; }
}

public sealed class PaymentIntentError
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
