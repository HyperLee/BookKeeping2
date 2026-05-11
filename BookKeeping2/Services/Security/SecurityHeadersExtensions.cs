namespace BookKeeping2.Services.Security;

/// <summary>
/// Adds baseline security headers for the Razor Pages application.
/// </summary>
public static class SecurityHeadersExtensions
{
    /// <summary>
    /// Adds security headers that reduce clickjacking, MIME sniffing, referrer leakage and script injection risk.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseBookKeepingSecurityHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            IHeaderDictionary headers = context.Response.Headers;
            headers.XContentTypeOptions = "nosniff";
            headers.XFrameOptions = "DENY";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Content-Security-Policy"] =
                "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; base-uri 'self'; form-action 'self'";

            await next();
        });
    }
}
