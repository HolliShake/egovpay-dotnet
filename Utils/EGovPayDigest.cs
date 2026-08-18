using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace EGovPay.Utils;

/// <summary>
/// Computes the transaction "digest" required by the Generate Payment endpoint:
/// <c>hash_hmac('sha256', "{amount}|{txnid}", token)</c>, hex-encoded, per the
/// eGovPay API docs.
/// </summary>
public static class EGovPayDigest
{
    public static string Compute(double amount, string txnid, string token)
    {
        token = token.StartsWith("test_") || token.StartsWith("live_")
            ? token.Substring(5) 
            : token;

        Console.WriteLine(token);
        // 1. Trim token and transaction ID
        string cleanToken = token?.Trim() ?? string.Empty;
        string cleanTxnid = txnid?.Trim() ?? string.Empty;

        // 2. Format amount to 2 decimal places using InvariantCulture (e.g., "100.00")
        string formattedAmount = amount.ToString();

        // 3. Construct raw digest string: "$amount|$txnid"
        string payload = $"{formattedAmount}|{cleanTxnid}";

        // 4. Compute HMAC SHA-256
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(cleanToken));
        byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));

        // 5. Convert to lowercase hexadecimal
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
