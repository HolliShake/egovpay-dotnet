# EGovPay.Net

An unofficial .NET client for [eGovPay](https://egovpay.gov.ph), the Philippine government payment gateway. It's shaped after the [Stripe .NET SDK](https://github.com/stripe/stripe-dotnet) on purpose — a single `EGovPayClient`, resource services (`client.PaymentIntents`, `client.CheckoutSessions`, ...), typed models, retries, and webhook verification.

## Why "unofficial" and why the warnings below

eGovPay doesn't have a public API reference. The `/developers` page on their site is locked behind the [eGov API Developer Portal](https://platforms.e.gov.ph), which needs administrator approval, a use-case review, and a signed data-sharing agreement before you can even read the docs. I don't have that access, so I couldn't verify the actual endpoints, payloads, error shapes, or webhook signing scheme.

What I *could* verify is the OAuth exchange — eGovPay shares its authentication layer with the rest of the eGov API platform (SSO, eVerify, eMessage, etc.), and that flow is documented publicly at `e.gov.ph/developers`. Everything else here is a best guess modeled on how Stripe does it, clearly marked so you know what to check before relying on it:

| Piece | Status |
|---|---|
| OAuth token exchange (`partner_code` / `partner_secret` → bearer token) | Confirmed — matches the public eGov SSO docs |
| API base URL for eGovPay | Guessed (`api.egovpay.gov.ph`) |
| Resource paths (`/v1/payment_intents`, `/v1/checkout/sessions`, `/v1/refunds`, `/v1/customers`) | Guessed, Stripe-shaped |
| Request/response field names | Guessed |
| Error response shape | Guessed (`{ "error": { "code", "message" } }`) |
| Webhook signature scheme | Guessed (HMAC-SHA256 hex digest in an `EGovPay-Signature` header) |

Every guess is flagged with a `[PLACEHOLDER]` comment in the source, and mostly lives in one place: [`Utils/EGovPayEndpoints.cs`](Utils/EGovPayEndpoints.cs). If you get portal access, here's what to fix:

1. Swap the constants in `EGovPayEndpoints.cs` for the real base URL, resource paths, and header names.
2. Update the `[JsonPropertyName]` attributes in `Models/**` to match the actual schema.
3. Confirm the OAuth `scope` for payments — `EGovPayClientOptions.Scope` currently defaults to `"EGOVPAY"`, which is a guess.
4. Check whether the payments scope needs an `exchange_code` like the SSO flow does, or if it's a plain client-credentials grant. `OAuthTokenProvider` supports both already.
5. Rewrite `WebhookService.Verify` to match the real signing scheme once you know it.
6. Delete this section.

Everything else — the client structure, retry/idempotency handling, error mapping — isn't guesswork and should hold up fine on its own.

## Install

Not on NuGet. Reference the project directly, or pack it locally:

```bash
dotnet pack EGovPay.csproj -c Release
```

## Quickstart

```csharp
using EGovPay;
using EGovPay.Models.Payments;

var client = new EGovPayClient(new EGovPayClientOptions
{
    PartnerCode = "YOUR_PARTNER_CODE",
    PartnerSecret = "YOUR_PARTNER_SECRET",
    Environment = EGovPayEnvironment.Sandbox,
});

var intent = await client.PaymentIntents.CreateAsync(new PaymentIntentCreateOptions
{
    Amount = 150000, // PHP 1,500.00, in centavos
    Description = "Business Permit Renewal - BR-2026-00123",
    ReferenceNumber = "BR-2026-00123",
    AllowedPaymentChannels = new() { PaymentChannel.GCash, PaymentChannel.Card },
});

var confirmed = await client.PaymentIntents.ConfirmAsync(intent.Id, new PaymentIntentConfirmOptions
{
    PaymentChannel = PaymentChannel.GCash,
});

Console.WriteLine($"Send the payer to: {confirmed.CheckoutUrl}");
```

Or hand the whole payment UI to eGovPay with a hosted checkout session:

```csharp
var session = await client.CheckoutSessions.CreateAsync(new CheckoutSessionCreateOptions
{
    Amount = 150000,
    SuccessUrl = "https://your-agency-portal.gov.ph/payments/success",
    CancelUrl = "https://your-agency-portal.gov.ph/payments/cancel",
});

Console.WriteLine(session.Url); // redirect the payer here
```

A more complete walkthrough (refunds, webhook handling) is in [`Examples/Sample.cs`](Examples/Sample.cs).

## How it's laid out

```
EGovPayClient.cs           Entry point; owns the HttpClient + token cache, exposes the resource services
EGovPayClientOptions.cs    Configuration (credentials, environment, retries, webhook secret)
Auth/                      OAuth token exchange + caching against oauth.e.gov.ph
Http/                      Transport layer: retries, idempotency keys, error → exception mapping
Models/                    Request/response POCOs, grouped by resource
Services/                  One class per resource (PaymentIntents, CheckoutSessions, Refunds, Customers, Webhooks)
Exceptions/                EGovPayException hierarchy
Utils/                     EGovPayEndpoints (the placeholder registry) and query-string helpers
Examples/                  Runnable console sample
```

### Retries & idempotency

`POST`/`DELETE` requests get an `Idempotency-Key` header — a fresh GUID per logical call, reused across retries of that same call — so a network blip can't double-create a payment. Transient failures (`429`, `5xx`, connection errors) are retried with exponential backoff, up to `EGovPayClientOptions.MaxNetworkRetries` (default 2).

### Errors

Non-2xx responses raise `EGovPayApiException`, with the HTTP status, raw response body, and a best-effort parsed error code/message. Failures during the OAuth exchange raise `EGovPayAuthenticationException` instead.

### Webhooks

```csharp
var evt = client.ConstructWebhookEvent(rawRequestBody, request.Headers["EGovPay-Signature"]);
if (evt.Type == "payment_intent.succeeded")
{
    var intent = evt.Data.GetObject<PaymentIntent>();
    // ...
}
```

`ConstructWebhookEvent` throws `EGovPayException` if the signature is missing or doesn't match. Again — the signing scheme itself is unconfirmed, so treat this as a starting point, not a guarantee.

## Why bother shipping placeholders instead of waiting for real docs?

Because the OAuth layer is real and shared across every eGov API product, and the plumbing around it (retries, idempotency, error mapping, a Stripe-familiar shape) doesn't change once the actual payment schema shows up. This gets you that plumbing today, so the remaining work when you get portal access is filling in field names, not building a client from scratch.

## License

MIT — see [LICENSE](LICENSE). Not affiliated with or endorsed by the Philippine DICT, eGovPH, or eGovPay.
