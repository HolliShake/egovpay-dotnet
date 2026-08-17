using EGovPay.Auth;
using EGovPay.Http;
using EGovPay.Services;
using EGovPay.Utils;

namespace EGovPay;

/// <summary>
/// Entry point for the eGovPay SDK. Construct one instance per set of
/// credentials (typically a singleton per agency/integration) and reuse it —
/// it owns an <see cref="HttpClient"/> and a cached OAuth token, just like
/// Stripe.net's <c>StripeClient</c>.
///
/// <code>
/// var client = new EGovPayClient(new EGovPayClientOptions
/// {
///     PartnerCode = "YOUR_PARTNER_CODE",
///     PartnerSecret = "YOUR_PARTNER_SECRET",
/// });
///
/// var intent = await client.PaymentIntents.CreateAsync(new PaymentIntentCreateOptions
/// {
///     Amount = 150000, // PHP 1,500.00
///     Description = "Business Permit Renewal - BR-2026-00123",
///     ReferenceNumber = "BR-2026-00123",
/// });
/// </code>
///
/// IMPORTANT: this library ships with [PLACEHOLDER] endpoint paths and payload
/// shapes for every eGovPay-specific resource (PaymentIntents, Checkout
/// Sessions, Refunds, Customers, Webhooks) because eGovPay's API reference is
/// gated behind an approved developer-portal account. Only the shared eGov
/// OAuth flow (<see cref="Auth.OAuthTokenProvider"/>) is confirmed against
/// public documentation. See <see cref="Utils.EGovPayEndpoints"/> for exactly
/// what needs updating once you have real docs.
/// </summary>
public sealed class EGovPayClient : IDisposable
{
    private readonly EGovPayHttpClient _httpClient;
    private readonly OAuthTokenProvider _tokenProvider;

    public PaymentIntentService PaymentIntents { get; }
    public CheckoutSessionService CheckoutSessions { get; }
    public RefundService Refunds { get; }
    public CustomerService Customers { get; }

    /// <summary>Signing secret configured for webhook verification, if any (see <see cref="EGovPayClientOptions.WebhookSigningSecret"/>).</summary>
    public string? WebhookSigningSecret { get; }

    public EGovPayClient(EGovPayClientOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.PartnerCode))
            throw new ArgumentException("PartnerCode is required.", nameof(options));
        if (string.IsNullOrWhiteSpace(options.PartnerSecret))
            throw new ArgumentException("PartnerSecret is required.", nameof(options));

        _tokenProvider = new OAuthTokenProvider(
            options.PartnerCode,
            options.PartnerSecret,
            options.Scope,
            tokenUrl: options.OAuthBaseUrlOverride ?? EGovPayEndpoints.OAuthTokenUrl);

        var apiBaseUrl = options.ApiBaseUrlOverride ?? options.Environment switch
        {
            EGovPayEnvironment.Sandbox => EGovPayEndpoints.DefaultSandboxApiBaseUrl,
            _ => EGovPayEndpoints.DefaultApiBaseUrl,
        };

        _httpClient = new EGovPayHttpClient(
            apiBaseUrl,
            _tokenProvider,
            options.MaxNetworkRetries,
            options.EnableIdempotencyKeys,
            options.Timeout);

        WebhookSigningSecret = options.WebhookSigningSecret;

        PaymentIntents = new PaymentIntentService(_httpClient);
        CheckoutSessions = new CheckoutSessionService(_httpClient);
        Refunds = new RefundService(_httpClient);
        Customers = new CustomerService(_httpClient);
    }

    /// <summary>
    /// Verifies and parses an inbound webhook. Convenience wrapper around
    /// <see cref="WebhookService.ConstructEvent"/> that uses the secret
    /// configured on this client.
    /// </summary>
    public Models.Webhooks.WebhookEvent ConstructWebhookEvent(string rawBody, string? signatureHeaderValue)
    {
        if (string.IsNullOrEmpty(WebhookSigningSecret))
        {
            throw new InvalidOperationException(
                $"No {nameof(EGovPayClientOptions.WebhookSigningSecret)} was configured on this client. " +
                "Pass one in EGovPayClientOptions, or call WebhookService.ConstructEvent directly with an explicit secret.");
        }

        return WebhookService.ConstructEvent(rawBody, signatureHeaderValue, WebhookSigningSecret);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _tokenProvider.Dispose();
    }
}
