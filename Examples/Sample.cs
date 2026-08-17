using EGovPay;
using EGovPay.Exceptions;
using EGovPay.Models.Checkout;
using EGovPay.Models.Payments;
using EGovPay.Models.Refunds;

// -----------------------------------------------------------------------
// This sample shows the intended, Stripe-like ergonomics of the SDK.
// It will NOT run successfully against real eGovPay servers yet, because
// EGovPay.Net.Utils.EGovPayEndpoints still contains placeholder paths.
// Once you have real API docs, update that file (and the model classes),
// then this sample should work unmodified.
// -----------------------------------------------------------------------

var client = new EGovPayClient(new EGovPayClientOptions
{
    PartnerCode = Environment.GetEnvironmentVariable("EGOVPAY_PARTNER_CODE") ?? "YOUR_PARTNER_CODE",
    PartnerSecret = Environment.GetEnvironmentVariable("EGOVPAY_PARTNER_SECRET") ?? "YOUR_PARTNER_SECRET",
    Environment = EGovPayEnvironment.Sandbox,
});

try
{
    // 1) Direct PaymentIntent flow (you control the UI / channel selection).
    var intent = await client.PaymentIntents.CreateAsync(new PaymentIntentCreateOptions
    {
        Amount = 150000, // PHP 1,500.00, in centavos
        Description = "Business Permit Renewal - BR-2026-00123",
        ReferenceNumber = "BR-2026-00123",
        AgencyCode = "QC-BPLO",
        AllowedPaymentChannels = new List<PaymentChannel> { PaymentChannel.GCash, PaymentChannel.Card },
        Payer = new PaymentIntentPayer
        {
            Name = "Juan Dela Cruz",
            Email = "[email protected]",
            Mobile = "+639123123123",
        },
        ReturnUrl = "https://your-agency-portal.gov.ph/payments/return",
    });

    Console.WriteLine($"Created PaymentIntent {intent.Id}, status={intent.Status}");

    var confirmed = await client.PaymentIntents.ConfirmAsync(intent.Id, new PaymentIntentConfirmOptions
    {
        PaymentChannel = PaymentChannel.GCash,
    });

    Console.WriteLine($"Confirmed. Redirect payer to: {confirmed.CheckoutUrl}");

    // 2) Or, the hosted Checkout Session flow (eGovPay owns the payment UI).
    var session = await client.CheckoutSessions.CreateAsync(new CheckoutSessionCreateOptions
    {
        Amount = 150000,
        Description = "Business Permit Renewal - BR-2026-00123",
        ReferenceNumber = "BR-2026-00123",
        SuccessUrl = "https://your-agency-portal.gov.ph/payments/success",
        CancelUrl = "https://your-agency-portal.gov.ph/payments/cancel",
    });

    Console.WriteLine($"Checkout session ready: {session.Url}");

    // 3) Refunding a succeeded payment.
    var refund = await client.Refunds.CreateAsync(new RefundCreateOptions
    {
        PaymentIntentId = intent.Id,
        Reason = "Duplicate payment",
    });

    Console.WriteLine($"Refund {refund.Id} status={refund.Status}");
}
catch (EGovPayApiException ex)
{
    Console.WriteLine($"eGovPay API error ({ex.StatusCode}): {ex.Message}");
}
catch (EGovPayAuthenticationException ex)
{
    Console.WriteLine($"Authentication failed: {ex.Message}");
}

// -----------------------------------------------------------------------
// 4) Handling a webhook (e.g. inside an ASP.NET Core minimal API endpoint):
//
// app.MapPost("/webhooks/egovpay", async (HttpRequest request) =>
// {
//     using var reader = new StreamReader(request.Body);
//     var rawBody = await reader.ReadToEndAsync();
//     var signature = request.Headers["EGovPay-Signature"];
//
//     var evt = client.ConstructWebhookEvent(rawBody, signature);
//     if (evt.Type == "payment_intent.succeeded")
//     {
//         var paidIntent = evt.Data.GetObject<PaymentIntent>();
//         // mark the invoice/permit/application as paid using paidIntent.ReferenceNumber
//     }
//
//     return Results.Ok();
// });
// -----------------------------------------------------------------------
