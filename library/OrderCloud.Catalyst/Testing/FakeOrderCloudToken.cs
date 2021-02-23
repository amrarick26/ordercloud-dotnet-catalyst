﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Flurl.Http;
using Microsoft.IdentityModel.Tokens;

namespace OrderCloud.Catalyst
{
    public static class FakeOrderCloudToken
    {
	    public static string Create(string clientID, List<string> roles = null) {
			roles = roles ?? new List<string>();
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("blahblahblahblahblahblahblahblahblahblah"));
		    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		    var token = new JwtSecurityToken(
			    issuer: "mydomain.com",
			    audience: "mydomain.com",
			    claims: new[] { new Claim("cid", clientID), new Claim("role", "shopper"), new Claim("role", "soemthing") },
			    expires: DateTime.Now.AddMinutes(30),
			    signingCredentials: creds);

		    return new JwtSecurityTokenHandler().WriteToken(token);
	    }

	    public static IFlurlClient WithFakeOrderCloudToken(this IFlurlClient fc, string clientId, List<string> roles = null) {
			roles = roles ?? new List<string>();
			return fc.WithOAuthBearerToken(Create(clientId));
	    }
	}
}
