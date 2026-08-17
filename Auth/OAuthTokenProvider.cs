using System.Text;
using System.Text.Json;
using EGovPay.Exceptions;
using EGovPay.Utils;

namespace EGovPay.Auth;

/// <summary>
/// Exchanges partner_code/partner_secret for a bearer access_token against the
/// shared eGov OAuth host, and caches it until shortly before it expires.
///
/// Based on the publicly confirmed eGov SSO API flow:
///   POST https://oauth.e.gov.ph/api/token
///   form: partner_code, partner_secret, scope, exchange_code
///
/// [PLACEHOLDER] The documented example always includes an `exchange_code`,
/// which for SSO represents a one-time code handed off from a signed-in user's
/// session. It's unclear whether eGovPay's server-to-server payment scope needs
/// one too, or whether a pure client-credentials grant (partner_code + secret
/// + scope, no exchange_code) is used instead. This implementation supports
/// both: pass an <c>exchangeCodeProvider</c> if your scope requires one,
/// otherwise the field is simply omitted from the request.
/// </summary>
public sealed class OAuthTokenProvider : IOAuthTokenProvider, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _partnerCode;
    private readonly string _partnerSecret;
    private readonly string _scope;
    private readonly Func<CancellationToken, Task<string?>>? _exchangeCodeProvider;
    private readonly string _tokenUrl;
    private readonly bool _ownsHttpClient;

    private static readonly TimeSpan RefreshSkew = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan FallbackLifetime = TimeSpan.FromMinutes(60);

    private readonly SemaphoreSlim _lock = new(1, 1);
    private OAuthToken? _cachedToken;

    public OAuthTokenProvider(
        string partnerCode,
        string partnerSecret,
        string scope,
        Func<CancellationToken, Task<string?>>? exchangeCodeProvider = null,
        string? tokenUrl = null,
        HttpClient? httpClient = null)
    {
        _partnerCode = partnerCode;
        _partnerSecret = partnerSecret;
        _scope = scope;
        _exchangeCodeProvider = exchangeCodeProvider;
        _tokenUrl = tokenUrl ?? EGovPayEndpoints.OAuthTokenUrl;
        _ownsHttpClient = httpClient is null;
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedToken is { } cached && !cached.IsExpiringSoon(RefreshSkew))
        {
            return cached.AccessToken;
        }

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_cachedToken is { } stillCached && !stillCached.IsExpiringSoon(RefreshSkew))
            {
                return stillCached.AccessToken;
            }

            _cachedToken = await FetchTokenAsync(cancellationToken).ConfigureAwait(false);
            return _cachedToken.AccessToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Invalidate() => _cachedToken = null;

    private async Task<OAuthToken> FetchTokenAsync(CancellationToken cancellationToken)
    {
        var formFields = new Dictionary<string, string>
        {
            ["partner_code"] = _partnerCode,
            ["partner_secret"] = _partnerSecret,
            ["scope"] = _scope,
        };

        if (_exchangeCodeProvider is not null)
        {
            var exchangeCode = await _exchangeCodeProvider(cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(exchangeCode))
            {
                formFields["exchange_code"] = exchangeCode!;
            }
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, _tokenUrl)
        {
            Content = new FormUrlEncodedContent(formFields)
        };
        request.Headers.UserAgent.TryParseAdd(EGovPayEndpoints.SdkUserAgent);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new EGovPayAuthenticationException(
                "Failed to reach the eGov OAuth token endpoint. Check network connectivity and the configured token URL.",
                ex);
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new EGovPayAuthenticationException(
                $"OAuth token exchange failed with HTTP {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}");
        }

        OAuthToken token;
        try
        {
            token = JsonSerializer.Deserialize<OAuthToken>(body)
                    ?? throw new EGovPayAuthenticationException("OAuth token response was empty.");
        }
        catch (JsonException ex)
        {
            throw new EGovPayAuthenticationException(
                $"Could not parse OAuth token response as JSON. Body: {body}", ex);
        }

        token.ExpiresAtUtc = TryReadJwtExpiry(token.AccessToken) ?? DateTimeOffset.UtcNow + FallbackLifetime;
        return token;
    }

    /// <summary>
    /// Best-effort decode of a JWT's `exp` claim without pulling in a full JWT
    /// library — we only need the expiry for cache invalidation, never signature
    /// verification (the server is the source of truth for token validity).
    /// </summary>
    private static DateTimeOffset? TryReadJwtExpiry(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2) return null;

            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            using var doc = JsonDocument.Parse(payloadJson);
            if (doc.RootElement.TryGetProperty("exp", out var expElement) && expElement.TryGetInt64(out var expSeconds))
            {
                return DateTimeOffset.FromUnixTimeSeconds(expSeconds);
            }
        }
        catch
        {
            // Non-fatal: fall back to FallbackLifetime in the caller.
        }
        return null;
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Convert.FromBase64String(s);
    }

    public void Dispose()
    {
        _lock.Dispose();
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }
}
