# EGovPay.Net

An **unofficial**, community-shaped .NET / C# client library for the [eGovPay](https://egovpay.gov.ph) government payment gateway (Philippines), built with the same ergonomics as the [Stripe .NET SDK](https://github.com/stripe/stripe-dotnet): a single `EGovPayClient`, resource-scoped services (`client.PaymentIntents`, `client.CheckoutSessions`, ...), strongly-typed request/response models, automatic retries, and webhook signature verification.

## ⚠️ Read this before you use it

**eGovPay does not publish a public API reference.** Its `/developers` page is a JS app gated behind an approved [eGov API Developer Portal](https://platforms.e.gov.ph) account — administrator approval, a use-case review, and a signed data-sharing agreement are required just to see the docs. I could not access real endpoint paths, request/response schemas, error formats, or the webhook signing scheme.

So this library is a **scaffold, not a finished integration**:

| Piece | Status |
|---|---|
| OAuth token exchange (`partner_code`/`partner_secret` → bearer token) | ✅ Confirmed against the publicly reachable eGov SSO docs at `e.gov.ph/developers` — this part is real and shared across eGov API products. |
| API base URL for eGovPay itself | ❌ Placeholder guess (`api.egovpay.gov.ph`) |
| Resource paths (`/v1/payment_intents`, `/v1/checkout/sessions`, `/v1/refunds`, `/v1/customers`, ...) | ❌ Placeholder, Stripe-shaped guesses |
| Request/response field names | ❌ Placeholder, Stripe-shaped guesses |
| Error response shape | ❌ Placeholder (assumes `{ "error": { "code", "message" } }`) |
| Webhook signature scheme | ❌ Placeholder (assumes HMAC-SHA256 hex digest in an `EGovPay-Signature` header) |

Every placeholder is called out with a `[PLACEHOLDER]` comment in code and centralized as much as possible in **[`src/EGovPay.Net/Utils/EGovPayEndpoints.cs`](src/EGovPay.Net/Utils/EGovPayEndpoints.cs)**. Once you get portal access:

1. Update the constants in `EGovPayEndpoints.cs` (base URL, resource paths, header names).
2. Fix up the `[JsonPropertyName]` attributes in `Models/**` to match the real schema.
3. Confirm the OAuth `scope` value for payments (`EGovPayClientOptions.Scope` currently guesses `"EGOVPAY"`).
4. Confirm whether the payments scope needs an `exchange_code` like the SSO flow does, or is a pure client-credentials grant (`OAuthTokenProvider` already supports both).
5. Update `WebhookService.Verify` to match the real signing scheme.
6. Delete this warning. 🙂

Treat everything else — the client structure, retry/idempotency handling, pagination shape, testability — as production-quality scaffolding you can build on with confidence.

## Install

This isn't published to NuGet (yet — see the caveats above). Reference the project directly, or pack it locally:

```bash
dotnet pack src/EGovPay.Net/EGovPay.Net.csproj -c Release
```

## Quickstart

```csharp
using EGovPay.Net;
using EGovPay.Net.Models.Payments;

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

Or the hosted-checkout flow:

```csharp
var session = await client.CheckoutSessions.CreateAsync(new CheckoutSessionCreateOptions
{
    Amount = 150000,
    SuccessUrl = "https://your-agency-portal.gov.ph/payments/success",
    CancelUrl = "https://your-agency-portal.gov.ph/payments/cancel",
});

Console.WriteLine(session.Url); // redirect the payer here
```

See [`samples/EGovPay.Net.Sample/Program.cs`](samples/EGovPay.Net.Sample/Program.cs) for a full walkthrough, including refunds and webhook handling.

## Design overview

```
src/EGovPay.Net/
  EGovPayClient.cs           Entry point; owns the HttpClient + token cache, exposes resource services
  EGovPayClientOptions.cs    Configuration (credentials, environment, retries, webhook secret)
  Auth/                      OAuth token exchange + caching against oauth.e.gov.ph
  Http/                      Low-level transport: retries, idempotency keys, error → exception mapping
  Models/                    Request/response POCOs, grouped by resource
  Services/                  One class per resource (PaymentIntents, CheckoutSessions, Refunds, Customers, Webhooks)
  Exceptions/                EGovPayException hierarchy
  Utils/                     EGovPayEndpoints (the placeholder registry), query-string helpers
samples/EGovPay.Net.Sample/  Runnable console sample
tests/EGovPay.Net.Tests/     xUnit tests (currently cover webhook signature verification)
```

### Retries & idempotency

Mutating requests (`POST`/`DELETE`) automatically attach an `Idempotency-Key` header (a fresh GUID per logical call, reused across retries of that same call) so a network blip can't double-create a payment. Transient failures (`429`, `5xx`, connection errors) are retried with exponential backoff up to `EGovPayClientOptions.MaxNetworkRetries` (default 2).

### Errors

All non-2xx responses raise `EGovPayApiException` with the HTTP status, raw response body, and a best-effort parsed error code/message. Auth failures during the OAuth exchange raise `EGovPayAuthenticationException`.

### Webhooks

```csharp
var evt = client.ConstructWebhookEvent(rawRequestBody, request.Headers["EGovPay-Signature"]);
if (evt.Type == "payment_intent.succeeded")
{
    var intent = evt.Data.GetObject<PaymentIntent>();
    // ...
}
```

`ConstructWebhookEvent` throws `EGovPayException` if the signature is missing or doesn't match — **again, the signing scheme itself is a placeholder** until confirmed.

## Why not just guess less?

An earlier draft of this README could have shipped zero payment-specific code and just said "go read the docs." Instead, this scaffold gives you a working OAuth layer (the one part that's genuinely confirmed), a Stripe-familiar shape to slot the real schema into, and tests/samples that already exercise the plumbing — so once you have portal access, the remaining work is filling in field names, not architecting a client from scratch.

## License

MIT — see [LICENSE](LICENSE). Not affiliated with or endorsed by the Philippine DICT, eGovPH, or eGovPay.
