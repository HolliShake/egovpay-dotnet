namespace EGovPay.Utils;

/// <summary>
/// Single source of truth for every URL/path this SDK calls, verified against
/// the public API reference at https://egovpay.gov.ph/developers.
/// </summary>
public static class EGovPayEndpoints
{
    /// <summary>
    /// Base URL for the transaction API. The docs' parameter table lists the host
    /// as "https://ws.egovpay.gov.ph", but every worked example (cURL, C#, etc.)
    /// on that same page targets "pgi-ws.egovpay.gov.ph" — use the latter since
    /// it's what the runnable samples actually call. Override via
    /// <see cref="EGovPayClientOptions.ApiBaseUrlOverride"/> if your account
    /// documentation says otherwise.
    /// </summary>
    public const string DefaultApiBaseUrl = "https://pgi-ws.egovpay.gov.ph";

    /// <summary>Create a transaction / generate a payment gateway link. POST.</summary>
    public const string TransactionsPath = "/api/v1/transaction";

    /// <summary>Retrieve a transaction by uuid. GET. Format with the uuid.</summary>
    public const string TransactionPath = "/api/v1/transaction/{0}";

    /// <summary>Void a transaction by uuid. PUT. Format with the uuid.</summary>
    public const string TransactionVoidPath = "/api/v1/transaction/{0}/void";

    /// <summary>Header used to authenticate every request. Value is the token issued in the merchant portal ("test_..." prefix for test mode).</summary>
    public const string TokenHeaderName = "X-eGovPay-Token";

    /// <summary>SDK identification header, sent as the User-Agent.</summary>
    public const string SdkUserAgent = "EGovPay.Net/0.1.0";
}
