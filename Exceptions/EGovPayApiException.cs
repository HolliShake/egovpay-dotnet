using System.Net;

namespace EGovPay.Exceptions;

/// <summary>
/// Thrown when the eGovPay API returns a non-2xx HTTP response. Mirrors the
/// shape of Stripe.net's StripeException: exposes the HTTP status, the raw
/// response body, and (best-effort) a parsed error code/message.
/// </summary>
public sealed class EGovPayApiException : EGovPayException
{
    public HttpStatusCode StatusCode { get; }

    /// <summary>Raw, unparsed response body — always available even if parsing fails.</summary>
    public string ResponseBody { get; }

    /// <summary>
    /// Machine-readable error code, if the API returned one. Field name assumed
    /// to be "code" or "error_code"; verify against real error payloads once
    /// available and adjust <see cref="Http.EGovPayHttpClient"/> parsing accordingly.
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>Correlates this failure with eGovPay's server-side logs, if provided.</summary>
    public string? RequestId { get; }

    public EGovPayApiException(
        HttpStatusCode statusCode,
        string responseBody,
        string message,
        string? errorCode = null,
        string? requestId = null)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
        ErrorCode = errorCode;
        RequestId = requestId;
    }
}