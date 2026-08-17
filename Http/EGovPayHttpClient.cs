using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EGovPay.Auth;
using EGovPay.Exceptions;
using EGovPay.Utils;

namespace EGovPay.Http;

/// <summary>
/// Default <see cref="IEGovPayHttpClient"/> implementation: attaches OAuth
/// bearer auth, retries transient failures with exponential backoff, and
/// translates non-2xx responses into <see cref="EGovPayApiException"/>.
/// </summary>
public sealed class EGovPayHttpClient : IEGovPayHttpClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly IOAuthTokenProvider _tokenProvider;
    private readonly string _baseUrl;
    private readonly int _maxNetworkRetries;
    private readonly bool _enableIdempotencyKeys;
    private readonly bool _ownsHttpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
    };

    public EGovPayHttpClient(
        string baseUrl,
        IOAuthTokenProvider tokenProvider,
        int maxNetworkRetries,
        bool enableIdempotencyKeys,
        TimeSpan timeout,
        HttpClient? httpClient = null)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _tokenProvider = tokenProvider;
        _maxNetworkRetries = Math.Max(0, maxNetworkRetries);
        _enableIdempotencyKeys = enableIdempotencyKeys;
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
        return SendAsync<TResponse>(HttpMethod.Post, path, content, options, isMutating: true);
    }

    public Task<TResponse> PostFormAsync<TResponse>(string path, IDictionary<string, string> form, RequestOptions? options = null)
    {
        HttpContent content = new FormUrlEncodedContent(form);
        return SendAsync<TResponse>(HttpMethod.Post, path, content, options, isMutating: true);
    }

    public Task<TResponse> DeleteAsync<TResponse>(string path, RequestOptions? options = null) =>
        SendAsync<TResponse>(HttpMethod.Delete, path, content: null, options, isMutating: true);

    private async Task<TResponse> SendAsync<TResponse>(
        HttpMethod method,
        string path,
        HttpContent? content,
        RequestOptions? options,
        bool isMutating = false)
    {
        var cancellationToken = options?.CancellationToken ?? default;
        var url = _baseUrl + path;

        // Idempotency key: reuse the same one across retries of the *same* logical
        // call so a network retry can't double-create a payment on eGovPay's side.
        string? idempotencyKey = null;
        if (isMutating && _enableIdempotencyKeys)
        {
            idempotencyKey = options?.IdempotencyKey ?? Guid.NewGuid().ToString("N");
        }

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

            var accessToken = await _tokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.UserAgent.TryParseAdd(EGovPayEndpoints.SdkUserAgent);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (idempotencyKey is not null)
            {
                request.Headers.TryAddWithoutValidation(EGovPayEndpoints.IdempotencyHeaderName, idempotencyKey);
            }

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

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Token might have been revoked/expired server-side ahead of our
                // local clock; force a refresh and retry once more if budget allows.
                _tokenProvider.Invalidate();
                if (attempt < _maxNetworkRetries)
                {
                    lastError = BuildApiException(response.StatusCode, body);
                    continue;
                }
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
    /// [PLACEHOLDER] Best-effort parse of eGovPay's error response shape.
    /// Assumes a Stripe-like `{ "error": { "code": "...", "message": "..." } }`
    /// envelope with a couple of fallbacks; update once the real schema is known.
    /// </summary>
    private static EGovPayApiException BuildApiException(HttpStatusCode statusCode, string body)
    {
        string? code = null;
        string? requestId = null;
        string message = $"eGovPay API request failed with HTTP {(int)statusCode}.";

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.TryGetProperty("error", out var errorEl))
            {
                if (errorEl.ValueKind == JsonValueKind.Object)
                {
                    if (errorEl.TryGetProperty("message", out var msgEl)) message = msgEl.GetString() ?? message;
                    if (errorEl.TryGetProperty("code", out var codeEl)) code = codeEl.GetString();
                }
                else if (errorEl.ValueKind == JsonValueKind.String)
                {
                    message = errorEl.GetString() ?? message;
                }
            }
            else if (root.TryGetProperty("message", out var topLevelMsg))
            {
                message = topLevelMsg.GetString() ?? message;
            }

            if (root.TryGetProperty("request_id", out var reqIdEl))
            {
                requestId = reqIdEl.GetString();
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

        return new EGovPayApiException(statusCode, body, message, code, requestId);
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
