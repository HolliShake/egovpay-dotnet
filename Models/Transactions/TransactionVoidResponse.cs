
using System.Text.Json.Serialization;

namespace EGovPay.Models.Transactions;

public class TransactionVoidResponse
{
    [JsonPropertyName("data")]
    public TransactionVoidResponseData Data { get; set; }
}

public class TransactionVoidResponseData
{
    [JsonPropertyName("message")]
    public string Message { get; set; }
}