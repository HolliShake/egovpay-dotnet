using System.Text.Json.Serialization;

namespace EGovPay.Auth;

/// <summary>
/// Response shape of POST https://oauth.e.gov.ph/api/token, confirmed against
/// the publicly documented eGov SSO API example response. The token is a JWT;
/// this SDK also decodes its `exp` claim so it can proactively refresh before
/// expiry instead of waiting for a 401.
/// </summary>
internal sealed class OAuthToken
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Absolute UTC expiry, decoded from the JWT's `exp` claim (seconds since
    /// epoch). Populated by <see cref="OAuthTokenProvider"/> after fetching.
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset ExpiresAtUtc { get; set; }

    /// <summary>True once we're within the refresh skew window of expiry.</summary>
    public bool IsExpiringSoon(TimeSpan skew) => DateTimeOffset.UtcNow >= ExpiresAtUtc - skew;
}