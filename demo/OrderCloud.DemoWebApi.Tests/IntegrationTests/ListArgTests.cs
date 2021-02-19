﻿using Flurl.Http;
using NUnit.Framework;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using SampleApp.WebApi.Controllers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OrderCloud.DemoWebApi.Tests
{
	[TestFixture]
	public class ListArgTests
	{
		private async Task<HttpResponseMessage> QueryListArgsRoute(string query)
		{
			return await TestFramework.Client.GetResponseAsync($"demo/listargs?{query}");
		}

		[TestCase("", 1)] // default page is 1
		[TestCase("pageSize=2&anything=random", 1)]
		[TestCase("page=1", 1)]
		[TestCase("page=10", 10)]
		[TestCase("page=47383", 47383)]
		[TestCase("page=15&pageSize=2&anything=random", 15)]
		public async Task page_should_deserialize_if_valid(string query, int expectedPage)
		{
			var response = await QueryListArgsRoute(query);
			response.ShouldHaveStatusCode(200);
			var args = await response.DeserializeAsync<ListArgs<ExampleModel>>();
			Assert.AreEqual(expectedPage, args.Page);
		}

		[TestCase("page=0")]
		[TestCase("page=1.1")]
		[TestCase("page=-1")]
		[TestCase("page=-3825")]
		[TestCase("page=-1&pageSize=2&anything=random")]
		[TestCase("page=-3825&pageSize=2&anything=random")]
		[TestCase("page=0x")]
		[TestCase("page=text")]
		[TestCase("page=12!&pageSize=2")]
		[TestCase("page=text&anything=random")]
		public async Task page_should_throw_error_if_invalid(string query)
		{
			var response = await QueryListArgsRoute(query);
			response.ShouldBeApiError("InvalidRequest", 400, "page must be an integer greater than or equal to 1.");
		}

		[TestCase("", 20)] // default page is 20
		[TestCase("pageSize=1", 1)]
		[TestCase("pageSize=2", 2)]
		[TestCase("pageSize=43", 43)]
		[TestCase("pageSize=100", 100)]
		public async Task page_size_should_deserialize_if_valid(string query, int expectedPageSize)
		{
			var response = await QueryListArgsRoute(query);
			response.ShouldHaveStatusCode(200);
			var args = await response.DeserializeAsync<ListArgs<ExampleModel>>();
			Assert.AreEqual(expectedPageSize, args.PageSize);
		}

		[TestCase("pageSize=0")]
		[TestCase("pageSize=1.1")]
		[TestCase("pageSize=101")]
		[TestCase("pageSize=-1")]
		[TestCase("pageSize=-3825")]
		[TestCase("pageSize=0&page=2&anything=random")]
		[TestCase("pageSize=101&page=2&anything=random")]
		[TestCase("pageSize=0x")]
		[TestCase("pageSize=text")]
		[TestCase("pageSize=12!&page=2")]
		[TestCase("pageSize=text&anything=random")]
		public async Task page_size_should_throw_error_if_invalid(string query)
		{
			var response = await QueryListArgsRoute(query);
			response.ShouldBeApiError("InvalidRequest", 400, "pageSize must be an integer between 1 and 100.");
		}

		public async Task sort_by_should_deserialize_if_valid(string query)
		{

		}
	}
}
