using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace AdventureWorks.IntegrationTests.Setup;

/// <summary>
/// Test authentication handler that replaces the production JWT Bearer scheme in the
/// integration test host.
/// </summary>
/// <remarks>
/// <para>
/// By default every request is authenticated: the handler returns
/// <see cref="AuthenticateResult.Success"/> with a synthetic principal carrying the claims
/// defined in <see cref="TestConstants"/>. This allows tests to reach protected endpoints
/// without issuing real tokens.
/// </para>
/// <para>
/// To simulate an unauthenticated request — and verify that <c>[Authorize]</c> responds with
/// HTTP 401 — add the <see cref="AnonymousHeader"/> to the request. The handler detects the
/// header and returns <see cref="AuthenticateResult.NoResult"/>, which causes the authorization
/// middleware to challenge the request.
/// </para>
/// Use <see cref="CustomWebApplicationFactory.CreateAnonymousClient"/> to obtain a client that
/// sends this header automatically.
/// </remarks>
[ExcludeFromCodeCoverage]
internal sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    /// <summary>The authentication scheme name registered in the test host's DI container.</summary>
    internal const string SchemeName = "Test";

    /// <summary>
    /// Request header that signals anonymous intent. When present,
    /// <see cref="HandleAuthenticateAsync"/> returns <see cref="AuthenticateResult.NoResult"/>
    /// instead of a success ticket.
    /// </summary>
    internal const string AnonymousHeader = "X-Test-Anonymous";

    /// <inheritdoc/>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers.ContainsKey(AnonymousHeader))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new[]
        {
            new Claim("oid", TestConstants.DefaultObjectId),
            new Claim("preferred_username", TestConstants.DefaultUpn),
            new Claim(ClaimTypes.Name, TestConstants.DefaultDisplayName),
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
