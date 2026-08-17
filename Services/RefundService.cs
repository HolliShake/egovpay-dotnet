using EGovPay.Http;
using EGovPay.Models.Common;
using EGovPay.Models.Refunds;
using EGovPay.Utils;

namespace EGovPay.Services;

/// <summary>[PLACEHOLDER] paths — see <see cref="EGovPayEndpoints"/>.</summary>
public sealed class RefundService
{
    private readonly IEGovPayHttpClient _http;

    internal RefundService(IEGovPayHttpClient http) => _http = http;

    public Task<Refund> CreateAsync(RefundCreateOptions options, RequestOptions? requestOptions = null) =>
        _http.PostAsync<Refund>(EGovPayEndpoints.RefundsPath, options, requestOptions);

    public Task<Refund> GetAsync(string refundId, RequestOptions? requestOptions = null) =>
        _http.GetAsync<Refund>(string.Format(EGovPayEndpoints.RefundPath, refundId), requestOptions);

    public Task<EGovPayList<Refund>> ListAsync(ListOptions? options = null, RequestOptions? requestOptions = null)
    {
        var path = QueryStringBuilder.Build(EGovPayEndpoints.RefundsPath, options?.ToQueryParameters());
        return _http.GetAsync<EGovPayList<Refund>>(path, requestOptions);
    }
}
