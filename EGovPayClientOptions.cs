namespace EGovPay;

/// <summary>
/// Configuration used to construct an <see cref="EGovPayClient"/>.
///
/// eGovPay authenticates requests with a single API token sent in the
/// <c>X-eGovPay-Token</c> header — there is no OAuth exchange. Whether the
/// token operates in test or live mode depends on the token itself (test
/// tokens are prefixed <c>test_</c>), not on any client-side setting.
/// </summary>
public sealed class EGovPayClientOptions
{
    /// <summary>
    /// API token issued in the eGovPay merchant portal. Test-mode tokens are
    /// prefixed <c>test_</c>; live tokens are not. Never expose this outside of
    /// trusted server-side code.
    /// </summary>
    public required string ApiToken { get; init; }

    /// <summary>
    /// Overrides the eGovPay API base URL. Defaults to
    /// <see cref="Utils.EGovPayEndpoints.DefaultApiBaseUrl"/>.
    /// </summary>
    public string? ApiBaseUrlOverride { get; init; }

    /// <summary>Maximum automatic retries for transient (network/5xx/429) failures.</summary>
    public int MaxNetworkRetries { get; init; } = 2;

    /// <summary>Per-request timeout.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
}

