namespace EGovPay;

/// <summary>
/// Which eGovPay/eGov API environment to target.
/// </summary>
public enum EGovPayEnvironment
{
    /// <summary>Live / production traffic. Real money moves.</summary>
    Production,

    /// <summary>
    /// Sandbox / staging traffic, if and when eGovPay exposes one.
    /// TODO: confirm the actual sandbox host once you have portal access —
    /// this currently falls back to the production host.
    /// </summary>
    Sandbox
}

/// <summary>
/// Configuration used to construct an <see cref="EGovPayClient"/>.
///
/// eGovPay sits on the shared "eGov API" platform (the same one that issues
/// SSO/eVerify/eMessage credentials), which authenticates partners via a
/// partner_code + partner_secret exchanged for a bearer access_token at
/// https://oauth.e.gov.ph/api/token. eGovPay-specific resource endpoints are
/// assumed to live on a sibling host (egovpay.gov.ph / api.egovpay.gov.ph) but
/// the exact base URL is NOT publicly documented — see
/// <see cref="Utils.EGovPayEndpoints"/> for the placeholder that needs updating.
/// </summary>
public sealed class EGovPayClientOptions
{
    /// <summary>
    /// Partner code issued by the eGov API administrator after your agency's
    /// integration is approved. Equivalent to a Stripe "publishable" identifier,
    /// but treated as sensitive here — do not ship it in client-side code.
    /// </summary>
    public required string PartnerCode { get; init; }

    /// <summary>
    /// Partner secret issued alongside <see cref="PartnerCode"/>. Never expose
    /// this outside of trusted server-side code.
    /// </summary>
    public required string PartnerSecret { get; init; }

    /// <summary>
    /// OAuth "scope" to request when exchanging credentials for an access token.
    /// TODO: confirm the correct scope string for payments — this placeholder
    /// mirrors the pattern documented for SSO (`SSO_AUTHENTICATION`). eGovPay's
    /// equivalent might be something like `EGOVPAY` or `PAYMENTS`.
    /// </summary>
    public string Scope { get; init; } = "EGOVPAY";

    /// <summary>
    /// Which environment to talk to. Defaults to <see cref="EGovPayEnvironment.Production"/>.
    /// </summary>
    public EGovPayEnvironment Environment { get; init; } = EGovPayEnvironment.Production;

    /// <summary>
    /// Overrides the OAuth token endpoint. Defaults to the publicly documented
    /// shared eGov OAuth host: https://oauth.e.gov.ph/api/token
    /// </summary>
    public string? OAuthBaseUrlOverride { get; init; }

    /// <summary>
    /// Overrides the eGovPay resource API base URL. Defaults to a PLACEHOLDER —
    /// see <see cref="Utils.EGovPayEndpoints"/>. Set this explicitly once you know
    /// the real host from the developer portal.
    /// </summary>
    public string? ApiBaseUrlOverride { get; init; }

    /// <summary>
    /// Optional idempotency-key generator hook. Stripe-style APIs (and most
    /// modern payment gateways) accept an Idempotency-Key header on mutating
    /// requests so retries don't double-charge. eGovPay's exact header name is
    /// unconfirmed; <see cref="Utils.EGovPayEndpoints.IdempotencyHeaderName"/>
    /// holds the current placeholder ("Idempotency-Key").
    /// </summary>
    public bool EnableIdempotencyKeys { get; init; } = true;

    /// <summary>Maximum automatic retries for transient (network/5xx/429) failures.</summary>
    public int MaxNetworkRetries { get; init; } = 2;

    /// <summary>Per-request timeout.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Secret used to verify inbound webhook signatures. Fill in once eGovPay's
    /// webhook signing scheme is documented (header name + HMAC algorithm are
    /// currently unconfirmed — see <see cref="Services.WebhookService"/>).
    /// </summary>
    public string? WebhookSigningSecret { get; init; }
}
