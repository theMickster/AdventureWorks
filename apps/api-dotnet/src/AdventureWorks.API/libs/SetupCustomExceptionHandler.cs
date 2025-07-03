using AdventureWorks.API.libs.Middleware;

namespace AdventureWorks.API.libs;

public static class SetupCustomExceptionHandler
{
    public static IApplicationBuilder UseAdventureWorksExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}
