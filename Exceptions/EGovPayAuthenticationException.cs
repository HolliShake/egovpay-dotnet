namespace EGovPay.Exceptions;

/// <summary>Base type for every exception this SDK throws.</summary>
public class EGovPayAuthenticationException : Exception
{
    public EGovPayAuthenticationException(string message) : base(message) { }

    public EGovPayAuthenticationException(string message, Exception innerException)
        : base(message, innerException) { }
}