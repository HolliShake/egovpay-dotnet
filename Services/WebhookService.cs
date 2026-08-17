using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EGovPay.Exceptions;
using EGovPay.Models.Webhooks;
using EGovPay.Utils;

namespace EGovPay.Services;

/// <summary>
/// Verifies and parses inbound eGovPay webhook deliveries, mirroring Stripe.net's
/// <c>EventUtility.ConstructEvent</c>.
///
/// [PLACEHOLDER] Everything about eGovPay's webhook signing scheme is
/// unconfirmed: the header name (<see cref="EGovPayEndpoints.WebhookSignatureHeaderName"/>),
/// the signed-payload format, and the HMAC algorithm. This implementation
/// assumes a Stripe-style `HMAC-SHA256(secret, rawBody)` hex digest sent as a
/// single header value. Update <see cref="Verify"/> once the real scheme is documented.
/// </summary>
public static class WebhookService
{
    /// <summary>
    /// Verifies the signature on a raw webhook request body and, if valid,
    /// deserializes it into a <see cref="WebhookEvent"/>.
    /// </summary>
    /// <param name="rawBody">The exact, unmodified request body bytes as received (do not re-serialize).</param>
    /// <param name="signatureHeaderValue">Value of the <see cref="EGovPayEndpoints.WebhookSignatureHeaderName"/> header.</param>
    /// <param name="signingSecret">Your webhook signing secret from the developer portal.</param>
    /// <exception cref="EGovPayException">Thrown if the signature is missing, malformed, or does not match.</exception>
    public static WebhookEvent ConstructEvent(string rawBody, string? signatureHeaderValue, string signingSecret)
    {
        Verify(rawBody, signatureHeaderValue, signingSecret);

        try
        {
            return JsonSerializer.Deserialize<WebhookEvent>(rawBody)
                   ?? throw new EGovPayException("Webhook payload deserialized to null.");
        }
        catch (JsonException ex)
        {
            throw new EGovPayException("Webhook signature was valid but the payload was not valid JSON.", ex);
        }
    }

    /// <summary>Signature check only, without deserializing — useful if you want to defer parsing.</summary>
    public static void Verify(string rawBody, string? signatureHeaderValue, string signingSecret)
    {
        if (string.IsNullOrEmpty(signatureHeaderValue))
        {
            throw new EGovPayException(
                $"Missing '{EGovPayEndpoints.WebhookSignatureHeaderName}' header on inbound webhook request.");
        }

        var expected = ComputeHmacSha256Hex(rawBody, signingSecret);

        // [PLACEHOLDER] Assumes the header carries the raw hex digest directly.
        // Stripe instead sends "t=...,v1=..." and requires extracting v1 plus a
        // timestamp-tolerance check to prevent replay — revisit this once
        // eGovPay's actual header format is known, and add a timestamp check if
        // one is available.
        if (!FixedTimeEquals(expected, signatureHeaderValue.Trim()))
        {
            throw new EGovPayException("Webhook signature verification failed: signature does not match expected value.");
        }
    }

    private static string ComputeHmacSha256Hex(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        var aBytes = Encoding.UTF8.GetBytes(a);
        var bBytes = Encoding.UTF8.GetBytes(b);
        if (aBytes.Length != bBytes.Length) return false;
        return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }
}
