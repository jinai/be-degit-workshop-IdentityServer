# Exercise 3: Backend access for MVC web

In this exercise we are going to configure so that our MVC Web can access our api backend service. To do this we are going to "upgrade" our client for the MVC web to the Hybrid flow. This means that we will include an authorization code in our response along with the id_token.

## Exercise 3.1: Modify the client in Identity Server

Now we going to modify the `mvcweb` client in our identity server configuration, so that it uses Hybrid grant type instead of Implicit.

### Step 1

Change the client configuration to this:

```C#
new Client
{
    ClientId = "mvcweb",
    ClientName = "MVC web",
    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,

    ClientSecrets =
    {
        new Secret("secret".Sha256())
    },
    
    // where to redirect to after login
    RedirectUris = {"http://localhost:5002/signin-oidc"},

    // where to redirect to after logout
    PostLogoutRedirectUris = {"http://localhost:5002/signout-callback-oidc"},

    AllowedScopes = new List<string>
    {
        IdentityServerConstants.StandardScopes.OpenId,
        IdentityServerConstants.StandardScopes.Profile,
        "api1"
    }
}
```

There are 3 important differences compared to Implicit:
* AllowedGrantType is changed to support HybridAndClientCredentials.
* We added a client secret to the client. This is used by the MVC web when it requests access tokens.
* We added the scope `api1` to our allowed scopes, which is the ApiResource for our api. 

## Exercise 3.2: Update the configuration in MVC web

Now we need to modify the OpenId Authentication Handler in our MVC web.

### Step 1

Update the configuration to the following:
```C#
 services.AddAuthentication(options =>
    {
        options.DefaultScheme = "Cookies";
        options.DefaultChallengeScheme = "oidc";
    })
    .AddCookie("Cookies")
    .AddOpenIdConnect("oidc", options =>
    {
        options.SignInScheme = "Cookies";

        options.Authority = "http://localhost:5000";
        options.RequireHttpsMetadata = false;
        options.ResponseType = "code id_token";
        options.GetClaimsFromUserInfoEndpoint = true;

        options.ClientId = "mvcweb";
        options.ClientSecret = "secret";
        options.SaveTokens = true;

        options.Scope.Add("api1");

    });
```

The big difference here is that we configured `options.ResponseType = "code id_token";`, which means that our application wants both an Authorization Code and an Id Token back when the user sign in. We also added the client secret and the scope `api1`. This will configure the OpenIdConnect handler to request the scope for the Api.

We also added `options.GetClaimsFromUserInfoEndpoint = true;`. This is needed if we want to get the identity information for the User. When using Hybrid flow, identity server is not adding any user claims to the Id Token. Instead we fetch them using the access token from the UserInfo endpoint. The access token is requested by the OpenId Handler from our identity server using the authorization code, which it then saves in the Auth Cookie (`options.SaveTokens = true;`).

It is also possible to configure IdentityServer4 to always send the user claims in the Id Token by adding the `AlwaysIncludeUserClaimsInIdToken = true` to the client configuration.

### Step 2

Now we need to use our access token in our call to the api backend. In our MVC web project there is an `MvcHttpService` that handles all calls to the backend. It is prepared with an `IHttpContextAccessor` service, so that we can access our token from the HttpContext. Modify the `PostAsync` method:

```C#
var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");

var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);

requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
requestMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");

var response = await _httpClient.SendAsync(requestMessage);

return response;
```

Here we fetch the access_token from our `HttpContext` and add it to the Authorization header in the request to our api.
