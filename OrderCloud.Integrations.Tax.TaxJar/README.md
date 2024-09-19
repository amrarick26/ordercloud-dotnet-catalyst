﻿# OrderCloud.Integrations.Tax.TaxJar

This project brings easy tax calculation to your ecommerce app using the [TaxJar API](https://developers.taxjar.com/api/reference/). It is published as a [nuget code library](https://www.nuget.org/packages/OrderCloud.Integrations.Tax.TaxJar) and conforms to standard tax interfaces published in the base library OrderCloud.Catalyst.

## Basics and Installation

1. If you haven't, please read [How to Calculate Tax with OrderCloud](https://ordercloud.io/knowledge-base/how-to-calculate-tax-with-ordercloud). In short, webhooks from the platform make requests to solution-custom API routes that contain tax calculation logic. 
2. This library can be installed in the context of a .NET API project that responds to those webhooks. If you already have a .NET API project, great. If not, you can [follow this guide](https://ordercloud.io/knowledge-base/start-dotnet-middleware-from-scratch). After you have published your API, you will need to configure OrderCloud to point its Integration Event webhooks at your API. 
3. In your .NET project, add the OrderCloud.Integrations.Tax.TaxJar nuget package with either the Visual Studio UI or the dotnet CLI.

```dotnet add package OrderCloud.Integrations.Tax.TaxJar```

## Authentication and Injection

You will need these configuration data points to authneticate to the TaxJar API - *BaseUrl*, and *APIToken*. Create an account with taxjar and get these from the admin portal.

```c#
var taxJarService = new TaxJarService(new TaxJarConfig()
{
	BaseUrl = "https://api.sandbox.taxjar.com/v2",
	APIToken = "...",
});
```

For efficient use of compute resources and clean code, create 1 TaxJarService object and make it available throughout your project using inversion of control dependency injection. 

```c#
services.AddSingleton<ITaxCalculator>(taxJarService);
services.AddSingleton<ITaxCodeProvider>(taxJarService);
```

Notice that the interfaces being used to register taxJarService are not specific to TaxJar. They are general to the domain of tax and come from the upstream OrderCloud.Catalyst package. 


## Usage 

Create routes that respond to the OrderCloud platform's Integration Event webhooks. Inject the tax interfaces like ITaxCalculator and use them within the logic of the route. It is not recommended to rely directly on TaxJarService anywhere. The layer of abstraction that ITaxCalculator provides decouples your code from TaxJar as a specific provider and hides some internal complexity of tax calculation.

```c#
public class CheckoutIntegrationEventController : CatalystController
{
	private readonly ITaxCalculator _taxCalculator;

	public CheckoutIntegrationEventController(ITaxCalculator taxCalculator)
	{
		// Inject interface. Implementation will depend on how services were registered, TaxJarService in this case.
		_taxCalculator = taxCalculator; 
	}

	....

	[HttpPost, Route("ordercalculate")] // route and method specified by OrderCloud platform
	[OrderCloudWebhookAuth] // Security feature to verifiy request came from Ordercloud.
	public async Task<OrderCalculateResponse> CalculateOrder([FromBody] OrderCalculatePayload<CheckoutConfig> payload)
	{
		// custom logic and mapping 

		var summary = new OrderSummaryForTax() { ... }
		OrderTaxCalculation taxCalculation = await _taxCalculator.CalculateEstimateAsync(summary);
		response.TaxTotal = calculation.TotalTax; // Populate Total Tax field on the Order

		...
	}

	[HttpPost, Route("ordersubmit")] // route and method specified by OrderCloud platform
	[OrderCloudWebhookAuth] // Security feature to verifiy request came from Ordercloud.
	public async Task<OrderSubmitResponse> HandleOrderSubmit([FromBody] OrderCalculatePayload<CheckoutConfig> payload)
	{
		// custom logic and mapping 

		var summary = new OrderSummaryForTax() { ... }
		await _taxCalculator.CommitTransactionAsync(summary);

		...
	}
}
```

This library also supports more complex cases that require mulitple tax accounts with different credentials. For example, in a franchise business model where each location is independent but all sell on one ecommerce solution. In that case, still inject one instance of TaxJarService exactly as above. You can provide empty strings for the fields. However, when you call methods on the interfaces, provide the optional `configOverride` parameter. 

```c#
TaxJarConfig configOverride = await FetchTaxAccountCredentials(supplierID);
var summary = new OrderSummaryForTax() { ... }
OrderTaxCalculation taxCalculation = await _taxCalculator.CalculateEstimateAsync(summary, configOverride);
```
