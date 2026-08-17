using EGovPay.Http;
using EGovPay.Models.Checkout;
using EGovPay.Models.Common;
using EGovPay.Utils;

namespace EGovPay.Services;

/// <summary>Hosted, redirect-based checkout flow. [PLACEHOLDER] paths — see <see cref="EGovPayEndpoints"/>.</summary>
public sealed class CheckoutSessionService
{
    private readonly IEGovPayHttpClient _http;

    internal CheckoutSessionService(IEGovPayHttpClient http) => _http = http;

    public Task<CheckoutSession> CreateAsync(CheckoutSessionCreateOptions options, RequestOptions? requestOptions = null) =>
        _http.PostAsync<CheckoutSession>(EGovPayEndpoints.CheckoutSessionsPath, options, requestOptions);

    public Task<CheckoutSession> GetAsync(string checkoutSessionId, RequestOptions? requestOptions = null) =>
        _http.GetAsync<CheckoutSession>(string.Format(EGovPayEndpoints.CheckoutSessionPath, checkoutSessionId), requestOptions);

    public Task<EGovPayList<CheckoutSession>> ListAsync(ListOptions? options = null, RequestOptions? requestOptions = null)
    {
        var path = QueryStringBuilder.Build(EGovPayEndpoints.CheckoutSessionsPath, options?.ToQueryParameters());
        return _http.GetAsync<EGovPayList<CheckoutSession>>(path, requestOptions);
    }
}
