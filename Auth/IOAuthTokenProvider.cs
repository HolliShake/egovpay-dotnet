namespace EGovPay.Auth;

/// <summary>
/// Supplies a valid bearer access token for calls to the eGovPay resource API,
/// transparently refreshing it as needed.
/// </summary>
public interface IOAuthTokenProvider
{
    /// <summary>Returns a currently-valid access token, fetching or refreshing one if necessary.</summary>
    public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>Forces the next call to fetch a brand-new token instead of reusing a cached one.</summary>
    public void Invalidate();
}