using System.Text.Json.Serialization;

namespace EGovPay.Models.Payments;

/// <summary>
/// [PLACEHOLDER] Lifecycle states for a PaymentIntent, modeled on Stripe's
/// PaymentIntent status machine since eGovPay's real state names aren't public.
/// Adjust the enum members/JSON strings to match the real API once confirmed.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentIntentStatus
{
    RequiresPaymentMethod,
    RequiresConfirmation,
    Processing,
    RequiresAction,
    Succeeded,
    Canceled,
    Failed,
}
