using EGovPay.Http;
using EGovPay.Models.Common;
using EGovPay.Models.Payments;
using EGovPay.Utils;

namespace EGovPay.Services;

/// <summary>
/// CRUD + lifecycle operations for PaymentIntents. Analogous to
/// <c>StripeClient.V1.PaymentIntents</c> in Stripe.net.
///
/// Every path this service calls is a [PLACEHOLDER] — see
/// <see cref="EGovPayEndpoints"/>.
/// </summary>
public sealed class PaymentIntentService
{
    private readonly IEGovPayHttpClient _http;

    internal PaymentIntentService(IEGovPayHttpClient http) => _http = http;

    public Task<PaymentIntent> CreateAsync(PaymentIntentCreateOptions options, RequestOptions? requestOptions = null) =>
        _http.PostAsync<PaymentIntent>(EGovPayEndpoints.PaymentIntentsPath, options, requestOptions);

    public Task<PaymentIntent> GetAsync(string paymentIntentId, RequestOptions? requestOptions = null) =>
        _http.GetAsync<PaymentIntent>(string.Format(EGovPayEndpoints.PaymentIntentPath, paymentIntentId), requestOptions);

    public Task<PaymentIntent> ConfirmAsync(string paymentIntentId, PaymentIntentConfirmOptions? options = null, RequestOptions? requestOptions = null) =>
        _http.PostAsync<PaymentIntent>(string.Format(EGovPayEndpoints.PaymentIntentConfirmPath, paymentIntentId), options, requestOptions);

    public Task<PaymentIntent> CancelAsync(string paymentIntentId, RequestOptions? requestOptions = null) =>
        _http.PostAsync<PaymentIntent>(string.Format(EGovPayEndpoints.PaymentIntentCancelPath, paymentIntentId), body: null, requestOptions);

    public Task<EGovPayList<PaymentIntent>> ListAsync(ListOptions? options = null, RequestOptions? requestOptions = null)
    {
        var path = QueryStringBuilder.Build(EGovPayEndpoints.PaymentIntentsPath, options?.ToQueryParameters());
        return _http.GetAsync<EGovPayList<PaymentIntent>>(path, requestOptions);
    }
}
