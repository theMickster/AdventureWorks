using AdventureWorks.API.Controllers.v1.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;

namespace AdventureWorks.UnitTests.API.Controllers;

/// <summary>
/// Security Rule #1: all write endpoints (POST/PUT/PATCH/DELETE) require authentication.
/// Program.cs registers no fallback or global authorization policy, so a write action that is
/// not covered by [Authorize] is reachable anonymously.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class WriteEndpointAuthorizationTests
{
    private static readonly string[] _readOnlyHttpMethods = ["GET", "HEAD", "OPTIONS"];

    [Fact]
    public void Every_write_action_requires_authorization_and_is_not_anonymous()
    {
        var writeActions = GetWriteActions();

        writeActions.Should().NotBeEmpty("because the scan must find the API's write endpoints, otherwise it would pass vacuously");

        var unprotected = writeActions
            .Where(action => !IsProtected(action))
            .Select(action => $"{action.DeclaringType!.Name}.{action.Name}")
            .Order()
            .ToList();

        unprotected.Should().BeEmpty("because every POST/PUT/PATCH/DELETE action must be covered by [Authorize] and must not be [AllowAnonymous]");
    }

    private static List<MethodInfo> GetWriteActions() =>
        typeof(SearchProductsController).Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(ControllerBase).IsAssignableFrom(type))
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(method => method.GetCustomAttributes<HttpMethodAttribute>(inherit: true)
                .SelectMany(attribute => attribute.HttpMethods)
                .Any(httpMethod => !_readOnlyHttpMethods.Contains(httpMethod, StringComparer.OrdinalIgnoreCase)))
            .ToList();

    private static bool IsProtected(MethodInfo action)
    {
        var controller = action.DeclaringType!;

        var isAnonymous = action.GetCustomAttributes<AllowAnonymousAttribute>(inherit: true).Any()
                          || controller.GetCustomAttributes<AllowAnonymousAttribute>(inherit: true).Any();

        var isAuthorized = action.GetCustomAttributes<AuthorizeAttribute>(inherit: true).Any()
                           || controller.GetCustomAttributes<AuthorizeAttribute>(inherit: true).Any();

        return isAuthorized && !isAnonymous;
    }
}
