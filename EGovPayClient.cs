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
///     {
///         TxnId = "USTP-ABCDEFG-2026-00123",
///         Items = [new TransactionItem { Name = "Item # 1", Amount = 1000.00 }],
///         SettlementTemplateUuid = Guid.NewGuid().ToString(),
///         RedirectUrl = "https://localhost:8001/",
///         CallbackUrl = "https://localhost:8000/callback",
///         Mobile = "+639455477865",
///         Email = "redondophilippandrewroa.dev@gmail.com",
///         Name = "TEST",
///         ExpiresAt = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd HH:mm:ss"),
///         LinkExpiresAt = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd HH:mm:ss"),
///     });
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
