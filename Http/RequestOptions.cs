namespace EGovPay.Http;

/// <summary>
/// Optional per-call overrides, mirroring Stripe.net's RequestOptions pattern.
/// </summary>
public sealed class RequestOptions
{
    /// <summary>
    /// Explicit idempotency key for this call. If omitted on a mutating request
    /// and <see cref="EGovPayClientOptions.EnableIdempotencyKeys"/> is true, the
    /// SDK generates one automatically.
    /// </summary>
    public string? IdempotencyKey { get; init; }

    /// <summary>Extra headers to attach to just this request.</summary>
    public IDictionary<string, string>? ExtraHeaders { get; init; }

    /// <summary>Per-call cancellation token.</summary>
    public CancellationToken CancellationToken { get; init; } = default;
}
