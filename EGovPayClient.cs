using EGovPay.Http;
using EGovPay.Services;
using EGovPay.Utils;

namespace EGovPay;

/// <summary>
/// Entry point for the eGovPay SDK. Construct one instance per API token
/// (typically a singleton per agency/integration) and reuse it — it owns an
/// <see cref="HttpClient"/>.
///
/// <code>
/// var client = new EGovPayClient(new EGovPayClientOptions
/// {
///     ApiToken = "test_YOUR_TOKEN",
/// });
///
/// var txn = await client.Transactions.CreateAsync(new TransactionCreateOptions
/// {
///     TxnId = "BR-2026-00123",
///     Amount = "1500",
///     Items = new() { new TransactionItem { Name = "Business Permit Renewal", Amount = "1500" } },
///     SettlementTemplateUuid = "YOUR_SETTLEMENT_TEMPLATE_UUID",
///     RedirectUrl = "https://your-agency-portal.gov.ph/payments/return",
///     CallbackUrl = "https://your-agency-portal.gov.ph/payments/callback",
/// });
/// </code>
/// </summary>
public sealed class EGovPayClient : IDisposable
{
    private readonly EGovPayHttpClient _httpClient;

    public TransactionService Transactions { get; }

    public EGovPayClient(EGovPayClientOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ApiToken))
            throw new ArgumentException("ApiToken is required.", nameof(options));

        var apiBaseUrl = options.ApiBaseUrlOverride ?? EGovPayEndpoints.DefaultApiBaseUrl;

        _httpClient = new EGovPayHttpClient(
            apiBaseUrl,
            options.ApiToken,
            options.MaxNetworkRetries,
            options.Timeout);

        Transactions = new TransactionService(_httpClient, options.ApiToken);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
