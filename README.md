# EGovPay.Net

An unofficial .NET client for [eGovPay](https://egovpay.gov.ph), the Philippine government payment gateway. It wraps the three endpoints documented at [egovpay.gov.ph/developers](https://egovpay.gov.ph/developers): generate a payment gateway link, check a transaction's details, and void a transaction.

## Authentication

eGovPay authenticates every request with a single API token sent in the `X-eGovPay-Token` header — there's no OAuth exchange. Whether the token runs in test or live mode depends on the token itself (test tokens are prefixed `test_`), not on any client-side setting.

```csharp
var client = new EGovPayClient(new EGovPayClientOptions
{
    ApiToken = "test_YOUR_TOKEN",
});
```

## Install

Not on NuGet. Reference the project directly, or pack it locally:

```bash
dotnet pack EGovPay.csproj -c Release
```

## Quickstart

```csharp
using EGovPay;
using EGovPay.Models.Transactions;

var client = new EGovPayClient(new EGovPayClientOptions
{
    ApiToken = "test_YOUR_TOKEN",
});

var txn = await client.Transactions.CreateAsync(new TransactionCreateOptions
{
    TxnId = "BR-2026-00123",
    Amount = "1500",
    Items = new() { new TransactionItem { Name = "Business Permit Renewal", Amount = "1500" } },
    SettlementTemplateUuid = "YOUR_SETTLEMENT_TEMPLATE_UUID",
    RedirectUrl = "https://your-agency-portal.gov.ph/payments/return",
    CallbackUrl = "https://your-agency-portal.gov.ph/payments/callback",
});

Console.WriteLine($"Transaction {txn.Uuid} created, status={txn.Status}");

var details = await client.Transactions.GetAsync(txn.Uuid!);

var voided = await client.Transactions.VoidAsync(txn.Uuid!);
```

The `digest` field the API requires (`hash_hmac('sha256', "{amount}|{txnid}", token)`) is computed for you by `TransactionService.CreateAsync` — you don't need to build it yourself.

A runnable walkthrough is in [`Examples/Sample.cs`](Examples/Sample.cs).

## How it's laid out

```
EGovPayClient.cs           Entry point; owns the HttpClient, exposes Transactions
EGovPayClientOptions.cs    Configuration (API token, base URL override, retries, timeout)
Http/                      Transport layer: retries, error → exception mapping
Models/Transactions/       Transaction, TransactionItem, TransactionCreateOptions
Services/                  TransactionService (create/get/void)
Exceptions/                EGovPayException hierarchy
Utils/                     EGovPayEndpoints (paths/host) and EGovPayDigest (HMAC helper)
Examples/                  Runnable console sample
```

### Retries

Transient failures (`429`, `5xx`, connection errors) are retried with exponential backoff, up to `EGovPayClientOptions.MaxNetworkRetries` (default 2).

### Errors

Non-2xx responses raise `EGovPayApiException` with the HTTP status and raw response body. `401`/`403` responses (an invalid or missing token) raise `EGovPayAuthenticationException` instead.

## What's confirmed vs. unconfirmed

The public docs page only documents the three endpoints below — it doesn't publish a response schema, error format, or webhook signing scheme, so those aren't guessed at here.

| Piece | Status |
|---|---|
| Auth via `X-eGovPay-Token` header | Confirmed |
| `POST /api/v1/transaction` (create), `GET /api/v1/transaction/{uuid}` (details), `PUT /api/v1/transaction/{uuid}/void` (void) | Confirmed |
| Request body fields | Confirmed, from the docs' parameter table and worked examples |
| `digest` formula (`hmac_sha256("{amount}|{txnid}", token)`) | Confirmed |
| Base host: `pgi-ws.egovpay.gov.ph` | The docs' parameter table lists `ws.egovpay.gov.ph`, but every worked example on the same page calls `pgi-ws.egovpay.gov.ph` — this SDK uses the latter since it's what's actually runnable. Override with `ApiBaseUrlOverride` if needed. |
| Response body schema | Not published. `Transaction` models the fields the API echoes back from the request plus `uuid`; anything else the API returns lands in `Transaction.ExtensionData`. |
| Error response shape | Not published. `EGovPayApiException.ResponseBody` always has the raw body regardless. |
| Webhooks | Not documented beyond a `callback_url` field — no signing scheme is published, so this SDK doesn't implement webhook verification. |

## License

MIT — see [LICENSE](LICENSE). Not affiliated with or endorsed by the Philippine DICT, eGovPH, or eGovPay.
