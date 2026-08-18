namespace EGovPay.Exceptions;

/// <summary>Thrown when the eGovPay API rejects the configured <c>X-eGovPay-Token</c> (HTTP 401/403).</summary>
public sealed class EGovPayAuthenticationException : EGovPayException
{
    public EGovPayAuthenticationException(string message) : base(message) { }

    public EGovPayAuthenticationException(string message, Exception innerException)
        : base(message, innerException) { }
}
