﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Payment.PayPal.Models;

namespace OrderCloud.Integrations.Payment.PayPal
{
    public class PayPalClient
    {
        #region Step 1: Get access token, and create order with Authorize intent. Return token and approve URL to client
        // https://developer.paypal.com/api/rest/authentication/
        public static async Task<string> GetAccessTokenAsync(PayPalConfig config)
        {
            var response = await config.BaseUrl
                .WithBasicAuth(config.ClientID, config.SecretKey)
                .AppendPathSegments("v1", "oauth2", "token")
                .PostUrlEncodedAsync(new
                {
                    grant_type = "client_credentials"
                });

            var tokenResponse = await response.GetJsonAsync<AuthTokenResponse>();
            return tokenResponse.access_token;
        }

        // https://developer.paypal.com/docs/api/orders/v2/#orders_create
        public static async Task<PayPalOrder> CreateAuthorizedOrderAsync(PayPalConfig config, PurchaseUnit purchaseUnit, string requestId)
        {
            var request = await config.BaseUrl
                .AppendPathSegments("v2", "checkout", "orders")
                .WithHeader("PayPal-Request-Id", requestId)
                .WithOAuthBearerToken(config.Token)
                .PostJsonAsync(new
                {
                    intent = "AUTHORIZE",
                    purchase_units = new List<PurchaseUnit>()
                    {
                        purchaseUnit
                    }
                });

            return await request.GetJsonAsync<PayPalOrder>();
        }
        #endregion

        #region Step 2: Authorize the order
        // https://developer.paypal.com/docs/api/orders/v2/#orders_authorize
        public static async Task<PayPalOrder> AuthorizePaymentForOrderAsync(PayPalConfig config, AuthorizeCCTransaction transaction)
        {
            // transaction.OrderID represents TransactionID from step 1, the PayPal OrderID
            var request = await config.BaseUrl
                .AppendPathSegments("v2", "checkout", "orders", transaction.OrderID, "authorize")
                .WithHeader("PayPal-Request-Id", transaction.RequestID)
                .WithOAuthBearerToken(config.Token)
                .PostJsonAsync(new {});

            return await request.GetJsonAsync<PayPalOrder>();
        }
        #endregion

        #region Step 3: Capture the order

        // https://developer.paypal.com/docs/api/payments/v2/#authorizations_capture
        public static async Task<PayPalOrder> CapturePaymentAsync(PayPalConfig config, FollowUpCCTransaction transaction)
        {
            // transaction.TransactionID represents TransactionID from step 2, the PayPal Authorization ID
            var request = await config.BaseUrl
                .AppendPathSegments("v2", "payments", "authorizations", transaction.TransactionID, "capture")
                .WithHeader("PayPal-Request-Id", transaction.RequestID)
                .WithOAuthBearerToken(config.Token)
                .PostJsonAsync(new { });

            return await request.GetJsonAsync<PayPalOrder>();
        }
        #endregion

        // https://developer.paypal.com/docs/api/payments/v2/#authorizations_void
        public static async Task VoidPaymentAsync(PayPalConfig config, FollowUpCCTransaction transaction)
        {
            var request = await config.BaseUrl
                .AppendPathSegments("v2", "payments", "authorizations", transaction.TransactionID, "void")
                .WithHeader("PayPal-Request-Id", transaction.RequestID)
                .WithOAuthBearerToken(config.Token)
                .PostJsonAsync(new { });

            var response = await request.GetJsonAsync();
        }

        // https://developer.paypal.com/docs/api/payments/v2/#captures_refund
        public static async Task<PayPalOrderReturn> RefundPaymentAsync(PayPalConfig config, FollowUpCCTransaction transaction)
        {
            var request = await config.BaseUrl
                .AppendPathSegments("v2", "payments", "captures", transaction.TransactionID, "refund")
                .WithHeader("PayPal-Request-Id", transaction.RequestID)
                .WithOAuthBearerToken(config.Token)
                .PostJsonAsync(new { });

            return await request.GetJsonAsync<PayPalOrderReturn>();
        }

        // https://developer.paypal.com/docs/api/payment-tokens/v3/#customer_payment-tokens_get
        public static async Task<PaymentTokenResponse> ListPaymentTokensAsync(PayPalConfig config, string customerID)
        {
            return await config.BaseUrl
                .AppendPathSegments("v3", "vault", "payment-tokens")
                .SetQueryParam("customer_id", customerID)
                .WithOAuthBearerToken(config.Token).GetJsonAsync<PaymentTokenResponse>();
        }

        // https://developer.paypal.com/docs/api/payment-tokens/v3/#payment-tokens_get
        public static async Task<PayPalPaymentToken> GetPaymentTokenAsync(PayPalConfig config, string tokenID)
        {
            return await config.BaseUrl
                .AppendPathSegments("v3", "vault", "payment-tokens", tokenID)
                .WithHeader("PayPal-Request-ID", "")
                .WithOAuthBearerToken(config.Token).GetJsonAsync<PayPalPaymentToken>();
        }

        public class AuthTokenResponse
        {
            public string scope { get; set; }
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string app_id { get; set; }
            public int expires_in { get; set; }
            public string nonce { get; set; }
        }
    }
}
