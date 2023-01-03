using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;

[assembly: InternalsVisibleTo("Authenticator.Test.UnitTests")]
namespace AdventureWorks.API.libs;

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

        app.UseAuthentication();

        app.ConfigureApplicatonHeaders(isDevelopment, _swaggerNonceString);

        app.UseCors();

        app.UseHsts();

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

        app.MapControllers();

        app.MapGet("/", () => $"hello world from {Environment.MachineName}");

        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            options.DocumentTitle = "Authenticator API";
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Authenticator API 1.0");
            RewriteSwaggerIndexHtml(options, _swaggerNonceString);
        });

        app.UseHealthChecks("/health");

        //app.UseApplicationHealthChecks();

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