using System.Text.Json;
using EGovPay;
using EGovPay.Exceptions;
using EGovPay.Models.Transactions;

var client = new EGovPayClient(new EGovPayClientOptions
{
    ApiToken = Environment.GetEnvironmentVariable("EGOVPAY_API_TOKEN") ?? "[TOKEN HERE]",
});

try
{
    var txn = await client.Transactions.CreateAsync(new TransactionCreateOptions
    {
        TxnId = "USTP-ABCDEFG-2026-00123",
        Items = [new TransactionItem { Name = "Item # 1", Amount = 1000.00 }],
        SettlementTemplateUuid = Guid.NewGuid().ToString(),
        RedirectUrl = "https://localhost:8001/",
        CallbackUrl = "https://localhost:8000/callback",
        Mobile = "+639455477865",
        Email = "redondophilippandrewroa.dev@gmail.com",
        Name = "TEST",
        ExpiresAt = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd HH:mm:ss"),
        LinkExpiresAt = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd HH:mm:ss"),
    });

    Console.WriteLine($"Created transaction {txn.Data.Uuid}");

    var fetched = await client.Transactions.GetAsync(txn.Data.Uuid!);
    Console.WriteLine($"Fetched transaction {JsonSerializer.Serialize(fetched.Data)}");

    // var voided = await client.Transactions.VoidAsync(txn.Data.Uuid!);
    // Console.WriteLine($"Voided transaction {voided.Data.Message}");
}
catch (EGovPayApiException ex)
{
    Console.WriteLine($"eGovPay API error ({ex.StatusCode}): {ex.Message} ! {ex.InnerException?.Message}");
}
catch (EGovPayAuthenticationException ex)
{
    Console.WriteLine($"Authentication failed: {ex.Message}");
}

