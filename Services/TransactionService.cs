using EGovPay.Http;
using EGovPay.Models.Transactions;
using EGovPay.Utils;

namespace EGovPay.Services;

/// <summary>
/// Create, retrieve, and void transactions — the three endpoints documented at
/// https://egovpay.gov.ph/developers.
/// </summary>
public sealed class TransactionService
{
    private readonly IEGovPayHttpClient _http;
    private readonly string _apiToken;

    internal TransactionService(IEGovPayHttpClient http, string apiToken)
    {
        _http = http;
        _apiToken = apiToken;
    }

    /// <summary>
    /// Generates a payment gateway link (<c>POST /api/v1/transaction</c>). The
    /// <c>digest</c> field is computed automatically as
    /// <c>hex(hmac_sha256("{amount}|{txnid}", apiToken))</c>, per the docs.
    /// </summary>
    public Task<TransactionResponse> CreateAsync(TransactionCreateOptions options, RequestOptions? requestOptions = null)
    {
        var computedAmount = options.Items.Sum(item => item.Amount);

        var digest = EGovPayDigest.Compute(computedAmount, options.TxnId, _apiToken);
        var payload = new
        {
            txnid = options.TxnId,
            items = options.Items,
            settlement_template_uuid = options.SettlementTemplateUuid,
            redirect_url = options.RedirectUrl,
            callback_url = options.CallbackUrl,
            digest,
            currency = options.Currency,
            mobile = options.Mobile,
            email = options.Email,
            name = options.Name,
            expires_at = options.ExpiresAt,
            link_expires_at = options.LinkExpiresAt,
            description = new {
                description = options.Description
            },
        };

        return _http.PostAsync<TransactionResponse>(EGovPayEndpoints.TransactionsPath, payload, requestOptions);
    }

    /// <summary>Retrieves a transaction's details (<c>GET /api/v1/transaction/{uuid}</c>).</summary>
    public Task<TransactionDetailResponse> GetAsync(string uuid, RequestOptions? requestOptions = null) =>
        _http.GetAsync<TransactionDetailResponse>(string.Format(EGovPayEndpoints.TransactionPath, uuid), requestOptions);

    /// <summary>Voids a transaction (<c>PUT /api/v1/transaction/{uuid}/void</c>).</summary>
    public Task<TransactionVoidResponse> VoidAsync(string uuid, RequestOptions? requestOptions = null) =>
        _http.PutAsync<TransactionVoidResponse>(string.Format(EGovPayEndpoints.TransactionVoidPath, uuid), body: null, requestOptions);
}
