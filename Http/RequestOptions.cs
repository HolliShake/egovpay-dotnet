namespace EGovPay.Http;

/// <summary>Optional per-call overrides.</summary>
public sealed class RequestOptions
{
    /// <summary>Extra headers to attach to just this request.</summary>
    public IDictionary<string, string>? ExtraHeaders { get; init; }

    /// <summary>Per-call cancellation token.</summary>
    public CancellationToken CancellationToken { get; init; } = default;
}
