namespace EGovPay.Utils;

/// <summary>
/// Single source of truth for every URL/path this SDK calls.
///
/// STATUS KEY (see comments on each member):
///   [CONFIRMED]   verified against the publicly reachable e.gov.ph developer docs
///   [PLACEHOLDER] not publicly documented; eGovPay gates its API reference behind
///                 an approved developer-portal login. Update these once you have
///                 access to https://platforms.e.gov.ph (eGovPay product page).
/// </summary>
public static class EGovPayEndpoints
{
    // ---- OAuth (shared eGov API platform) ------------------------------------

    /// <summary>[CONFIRMED] Token exchange endpoint used across eGov API products (SSO, eVerify, eMessage, eGovPay, ...).</summary>
    public const string OAuthTokenUrl = "https://oauth.e.gov.ph/api/token";

    // ---- eGovPay resource API --------------------------------------------------

    /// <summary>
    /// [PLACEHOLDER] Base URL for eGovPay resource calls (payments, checkout, refunds, customers).
    /// The marketing site lives at https://egovpay.gov.ph but the API host is not
    /// publicly confirmed. Common possibilities to verify against the real docs:
    ///   - https://api.egovpay.gov.ph
    ///   - https://egovpay.e.gov.ph/api
    /// Override via <see cref="EGovPayClientOptions.ApiBaseUrlOverride"/> until confirmed.
    /// </summary>
    public const string DefaultApiBaseUrl = "https://api.egovpay.gov.ph";

    /// <summary>[PLACEHOLDER] Sandbox host, if one exists.</summary>
    public const string DefaultSandboxApiBaseUrl = "https://sandbox.api.egovpay.gov.ph";

    /// <summary>[PLACEHOLDER] Create/List a PaymentIntent-style resource.</summary>
    public const string PaymentIntentsPath = "/v1/payment_intents";

    /// <summary>[PLACEHOLDER] Retrieve/Cancel a specific PaymentIntent. Format with the id.</summary>
    public const string PaymentIntentPath = "/v1/payment_intents/{0}";

    /// <summary>[PLACEHOLDER] Confirm a PaymentIntent (finalize with a chosen payment channel).</summary>
    public const string PaymentIntentConfirmPath = "/v1/payment_intents/{0}/confirm";

    /// <summary>[PLACEHOLDER] Cancel a PaymentIntent.</summary>
    public const string PaymentIntentCancelPath = "/v1/payment_intents/{0}/cancel";

    /// <summary>[PLACEHOLDER] Create/List hosted Checkout Sessions (redirect-based flow).</summary>
    public const string CheckoutSessionsPath = "/v1/checkout/sessions";

    /// <summary>[PLACEHOLDER] Retrieve a specific Checkout Session.</summary>
    public const string CheckoutSessionPath = "/v1/checkout/sessions/{0}";

    /// <summary>[PLACEHOLDER] Create/List refunds.</summary>
    public const string RefundsPath = "/v1/refunds";

    /// <summary>[PLACEHOLDER] Retrieve a specific refund.</summary>
    public const string RefundPath = "/v1/refunds/{0}";

    /// <summary>[PLACEHOLDER] Create/List customers/payers.</summary>
    public const string CustomersPath = "/v1/customers";

    /// <summary>[PLACEHOLDER] Retrieve/Update/Delete a specific customer.</summary>
    public const string CustomerPath = "/v1/customers/{0}";

    /// <summary>[PLACEHOLDER] List available payment channels/methods for the merchant account.</summary>
    public const string PaymentChannelsPath = "/v1/payment_channels";

    // ---- Headers ----------------------------------------------------------------

    /// <summary>[PLACEHOLDER] Header used for idempotent retries of mutating requests.</summary>
    public const string IdempotencyHeaderName = "Idempotency-Key";

    /// <summary>[PLACEHOLDER] Header eGovPay uses to sign outbound webhook payloads (name/scheme unconfirmed).</summary>
    public const string WebhookSignatureHeaderName = "EGovPay-Signature";

    /// <summary>[PLACEHOLDER] SDK identification header, mirrors Stripe's X-Stripe-Client-User-Agent pattern.</summary>
    public const string SdkUserAgent = "EGovPay.Net/0.1.0-alpha (+https://github.com/your-org/egovpay-net)";
}
