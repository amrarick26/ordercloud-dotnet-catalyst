﻿using System;
using System.Collections.Generic;
using System.Text;

namespace OrderCloud.Integrations.Payment.Stripe.Models
{
    // https://stripe.com/docs/api/payment_intents/create?lang=dotnet
    public class StripePaymentIntentRequest
    {
        /// <summary>
        /// REQUIRED A positive integer representing amount intended to be collected.
        /// Minimum value is 50 ($0.50)
        /// </summary>
        public int? amount { get; set; }
        /// <summary>
        /// REQUIRED Three-letter ISO currency code in lowercase.
        /// Supported currencies: https://stripe.com/docs/currencies
        /// </summary>
        public string currency { get; set; }
        public string payment_method { get; set; }
        // this is wrong:
        public string[] payment_method_types { get; set; }
        public string client_secret { get; set; }
        /// <summary>
        /// ID of the Customer this PaymentIntent belongs to, if one exists.
        /// </summary>
        public string customer { get; set; }
        /// <summary>
        /// on_session value indicates the customer is in the checkout flow of your app. An off_session value indicates the customer has exited out of your app
        /// </summary>
        public string setup_future_usage { get; set; }
    }

    public class StripePaymentIntentResponse
    {
        public string id { get; set; }
        //public int amount { get; set; }
        //public int amount_capturable { get; set; }
        public int amount_received { get; set; }
        public string payment_method { get; set; }
        //public string application { get; set; }
        //public int application_fee_amount { get; set; }
        //public AutomaticPaymentMethods automatic_payment_methods { get; set; }

        //public List<string> payment_method_types { get; set; }

    }

    public class AutomaticPaymentMethods
    {
        public bool enabled { get; set; }
    }
}