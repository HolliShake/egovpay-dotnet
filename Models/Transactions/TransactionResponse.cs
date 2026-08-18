
namespace EGovPay.Models.Transactions;

public class TransactionResponse
{
    public TransactionResponseData Data { get; set; }
}

public class TransactionResponseData
{
    public string Uuid { get; set; }
    public string Url { get; set; }
}
