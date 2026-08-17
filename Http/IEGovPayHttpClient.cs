namespace EGovPay.Http;

/// <summary>Low-level transport used by every resource service. Internal-ish, but public for testability/mocking.</summary>
public interface IEGovPayHttpClient
{
    Task<TResponse> GetAsync<TResponse>(string path, RequestOptions? options = null);

    Task<TResponse> PostAsync<TResponse>(string path, object? body, RequestOptions? options = null);

    Task<TResponse> PostFormAsync<TResponse>(string path, IDictionary<string, string> form, RequestOptions? options = null);

    Task<TResponse> DeleteAsync<TResponse>(string path, RequestOptions? options = null);
}
