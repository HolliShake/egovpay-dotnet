namespace EGovPay.Exceptions;

/// <summary>Base type for every exception this SDK throws.</summary>
public class EGovPayException : Exception
{
    public EGovPayException(string message) : base(message) { }

    public EGovPayException(string message, Exception innerException)
        : base(message, innerException) { }
}