using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EGovPay.Exceptions;
using EGovPay.Utils;

namespace EGovPay.Http;

/// <summary>
/// Default <see cref="IEGovPayHttpClient"/> implementation: attaches the
/// <c>X-eGovPay-Token</c> header, retries transient failures with exponential
/// backoff, and translates non-2xx responses into <see cref="EGovPayApiException"/>
/// (or <see cref="EGovPayAuthenticationException"/> for 401/403).
/// </summary>
public sealed class EGovPayHttpClient : IEGovPayHttpClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _apiToken;
    private readonly int _maxNetworkRetries;
    private readonly bool _ownsHttpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
    };

    public EGovPayHttpClient(
        string baseUrl,
        string apiToken,
        int maxNetworkRetries,
        TimeSpan timeout,
        HttpClient? httpClient = null)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _apiToken = apiToken;
        _maxNetworkRetries = Math.Max(0, maxNetworkRetries);
        _ownsHttpClient = httpClient is null;
        _httpClient = httpClient ?? new HttpClient();
        _httpClient.Timeout = timeout;
    }

    public Task<TResponse> GetAsync<TResponse>(string path, RequestOptions? options = null) =>
        SendAsync<TResponse>(HttpMethod.Get, path, content: null, options);

    public Task<TResponse> PostAsync<TResponse>(string path, object? body, RequestOptions? options = null)
    {
        HttpContent? content = body is null
            ? null
            : new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
        return SendAsync<TResponse>(HttpMethod.Post, path, content, options);
    }

    public Task<TResponse> PutAsync<TResponse>(string path, object? body, RequestOptions? options = null)
    {
        HttpContent? content = body is null
            ? null
            : new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
        return SendAsync<TResponse>(HttpMethod.Put, path, content, options);
    }

    private async Task<TResponse> SendAsync<TResponse>(
        HttpMethod method,
        string path,
        HttpContent? content,
        RequestOptions? options)
    {
        var cancellationToken = options?.CancellationToken ?? default;
        var url = _baseUrl + path;

        Exception? lastError = null;

        for (var attempt = 0; attempt <= _maxNetworkRetries; attempt++)
        {
            if (attempt > 0)
            {
                await Task.Delay(BackoffDelay(attempt), cancellationToken).ConfigureAwait(false);
            }

            using var request = new HttpRequestMessage(method, url);
            if (content is not null)
            {
                // HttpContent isn't safely reusable across retries once it's been
                // sent, so re-buffer it fresh for every attempt.
                request.Content = await CloneContentAsync(content, cancellationToken).ConfigureAwait(false);
            }

            request.Headers.TryAddWithoutValidation(EGovPayEndpoints.TokenHeaderName, _apiToken);
            request.Headers.UserAgent.TryParseAdd(EGovPayEndpoints.SdkUserAgent);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (options?.ExtraHeaders is not null)
            {
                foreach (var (key, value) in options.ExtraHeaders)
                {
                    request.Headers.TryAddWithoutValidation(key, value);
                }
            }

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && attempt < _maxNetworkRetries)
            {
                lastError = ex;
                continue; // transient network failure — retry
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            Console.WriteLine($"HTTP {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new EGovPayAuthenticationException(
                    $"eGovPay rejected the configured {EGovPayEndpoints.TokenHeaderName} (HTTP {(int)response.StatusCode}). Response: {body}");
            }

            if (IsRetryable(response.StatusCode) && attempt < _maxNetworkRetries)
            {
                lastError = BuildApiException(response.StatusCode, body);
                continue;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw BuildApiException(response.StatusCode, body);
            }

            if (typeof(TResponse) == typeof(EmptyResponse) || string.IsNullOrWhiteSpace(body))
            {
                return default!;
            }

            try
            {
                return JsonSerializer.Deserialize<TResponse>(body, JsonOptions)!;
            }
            catch (JsonException ex)
            {
                throw new EGovPayException($"Failed to parse eGovPay API response as {typeof(TResponse).Name}. Raw body: {body}", ex);
            }
        }

        throw lastError ?? new EGovPayException("Request failed after retries for an unknown reason.");
    }

    private static bool IsRetryable(HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.TooManyRequests || (int)statusCode >= 500;

    private static TimeSpan BackoffDelay(int attempt) =>
        TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt - 1));

    private static async Task<HttpContent> CloneContentAsync(HttpContent original, CancellationToken cancellationToken)
    {
        var bytes = await original.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        var clone = new ByteArrayContent(bytes);
        foreach (var header in original.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        return clone;
    }

    /// <summary>
    /// The docs don't publish an error response schema, so this best-effort
    /// parses a couple of common shapes (<c>{"message": "..."}</c>,
    /// <c>{"error": "..."}</c>) and otherwise falls back to the raw body.
    /// </summary>
    private static EGovPayApiException BuildApiException(HttpStatusCode statusCode, string body)
    {
        string message = $"eGovPay API request failed with HTTP {(int)statusCode}.";

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.TryGetProperty("message", out var msgEl) && msgEl.ValueKind == JsonValueKind.String)
            {
                message = msgEl.GetString() ?? message;
            }
            else if (root.TryGetProperty("error", out var errorEl) && errorEl.ValueKind == JsonValueKind.String)
            {
                message = errorEl.GetString() ?? message;
            }
        }
        catch (JsonException)
        {
            // Body wasn't JSON (HTML error page, plain text, etc.) — fall back to the raw body.
            if (!string.IsNullOrWhiteSpace(body))
            {
                message += $" Response: {body}";
            }
        }

        return new EGovPayApiException(statusCode, body, message);
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }
}

/// <summary>Marker type for calls (like DELETE) that return no meaningful body.</summary>
public sealed class EmptyResponse { }
