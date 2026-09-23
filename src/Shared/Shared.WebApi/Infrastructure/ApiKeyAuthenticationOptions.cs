using Microsoft.AspNetCore.Authentication;

namespace Shared.WebApi.Infrastructure;

public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "ApiKey";
    public const string HeaderName = "X-API-Key";
}
