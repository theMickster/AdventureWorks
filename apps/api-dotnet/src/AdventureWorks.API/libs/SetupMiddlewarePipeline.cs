using AdventureWorks.API.libs.Extensions;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;

[assembly: InternalsVisibleTo("AdventureWorks.UnitTests")]
namespace AdventureWorks.API.libs;

[ExcludeFromCodeCoverage]
internal static class SetupMiddlewarePipeline
{
    private static readonly string _swaggerNonceString = Guid.NewGuid().ToString("n");

    internal static WebApplication SetupMiddleware(this WebApplication app)
    {

        var isDevelopment = app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!.ToLowerInvariant().Contains("dev")
                                                            || app.Environment.EnvironmentName.EndsWith("dev") || app.Environment.EnvironmentName.EndsWith("local");

        if (isDevelopment)
        {
            app.UseDeveloperExceptionPage();
        }

        // Register correlation ID middleware FIRST to ensure all subsequent middleware and requests have access
        app.UseCorrelationId();

        app.ConfigureApplicatonHeaders(isDevelopment, _swaggerNonceString);

        app.UseResponseCompression();

        app.UseCors();

        app.UseHsts();

        app.UseAdventureWorksExceptionHandler();

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();

        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            options.DocumentTitle = "Adventure Works API";
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Adventure Works API 1.0");
            RewriteSwaggerIndexHtml(options, _swaggerNonceString);
        });

        app.UseHealthChecks("/health");

        app.UseApplicationHealthChecks();

        return app;
    }

    #region Private Methods

    /// <summary>
    /// Re-write the swagger index page adding a nonce
    /// </summary>
    /// <param name="options"></param>
    /// <param name="nonceString"></param>
    private static void RewriteSwaggerIndexHtml(SwaggerUIOptions options, string nonceString)
    {
        var originalIndexStreamFactory = options.IndexStream;

        options.IndexStream = () =>
        {
            using var originalStream = originalIndexStreamFactory();
            using var originalStreamReader = new StreamReader(originalStream);
            var originalIndexHtmlContents = originalStreamReader.ReadToEnd();

            var nonceEnabledIndexHtmlContents = originalIndexHtmlContents
                .Replace("<script>", $"<script nonce=\"{nonceString}\">", StringComparison.OrdinalIgnoreCase)
                .Replace("<style>", $"<style nonce=\"{nonceString}\">", StringComparison.OrdinalIgnoreCase);

            return new MemoryStream(Encoding.UTF8.GetBytes(nonceEnabledIndexHtmlContents));
        };
    }

    #endregion Private Methods
}