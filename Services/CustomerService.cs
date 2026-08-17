using EGovPay.Http;
using EGovPay.Models.Common;
using EGovPay.Models.Customers;
using EGovPay.Utils;

namespace EGovPay.Services;

/// <summary>[PLACEHOLDER] paths — see <see cref="EGovPayEndpoints"/>.</summary>
public sealed class CustomerService
{
    private readonly IEGovPayHttpClient _http;

    internal CustomerService(IEGovPayHttpClient http) => _http = http;

    public Task<Customer> CreateAsync(CustomerCreateOptions options, RequestOptions? requestOptions = null) =>
        _http.PostAsync<Customer>(EGovPayEndpoints.CustomersPath, options, requestOptions);

    public Task<Customer> GetAsync(string customerId, RequestOptions? requestOptions = null) =>
        _http.GetAsync<Customer>(string.Format(EGovPayEndpoints.CustomerPath, customerId), requestOptions);

    public Task<Customer> UpdateAsync(string customerId, CustomerCreateOptions options, RequestOptions? requestOptions = null) =>
        _http.PostAsync<Customer>(string.Format(EGovPayEndpoints.CustomerPath, customerId), options, requestOptions);

    public Task DeleteAsync(string customerId, RequestOptions? requestOptions = null) =>
        _http.DeleteAsync<EmptyResponse>(string.Format(EGovPayEndpoints.CustomerPath, customerId), requestOptions);

    public Task<EGovPayList<Customer>> ListAsync(ListOptions? options = null, RequestOptions? requestOptions = null)
    {
        var path = QueryStringBuilder.Build(EGovPayEndpoints.CustomersPath, options?.ToQueryParameters());
        return _http.GetAsync<EGovPayList<Customer>>(path, requestOptions);
    }
}
