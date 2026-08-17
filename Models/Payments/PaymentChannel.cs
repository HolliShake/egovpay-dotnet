using System.Text.Json.Serialization;

namespace EGovPay.Models.Payments;

/// <summary>
/// [PLACEHOLDER] Payment channels/rails eGovPay is likely to route through,
/// inferred from its marketing copy ("automatically route payments through the
/// most optimal channels") and common Philippine payment gateways (GCash,
/// Maya, cards, bank transfer/InstaPay/PESONet, over-the-counter). Replace
/// with the actual enumerated values from the real API docs.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentChannel
{
    Card,
    GCash,
    Maya,
    InstaPay,
    PesoNet,
    OverTheCounter,
    QrPh,
}
