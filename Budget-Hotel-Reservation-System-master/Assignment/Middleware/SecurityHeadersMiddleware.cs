namespace Assignment.Middleware
{
    /// <summary>
    /// Middleware that adds security headers to all HTTP responses.
    /// These headers help protect against common web vulnerabilities such as:
    /// - Clickjacking (X-Frame-Options)
    /// - Cross-site scripting (XSS) attacks (X-XSS-Protection, Content-Security-Policy)
    /// - MIME type sniffing (X-Content-Type-Options)
    /// - Information leakage (Referrer-Policy)
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        /// <summary>
        /// The next middleware in the request pipeline.
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the SecurityHeadersMiddleware.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invokes the middleware to add security headers to the HTTP response.
        /// </summary>
        /// <param name="context">The HTTP context for the current request.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            // Prevent the page from being displayed in an iframe (clickjacking protection)
            context.Response.Headers["X-Frame-Options"] = "DENY";

            // Enable browser's built-in XSS protection
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

            // Prevent MIME type sniffing (prevents browsers from guessing content types)
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";

            // Control how much referrer information is sent with requests
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Content-Security-Policy: Restricts which resources can be loaded
            // Permissive policy for assignment to avoid breaking scripts/styles
            // Allows:
            // - Scripts from self, cdn.jsdelivr.net (Chart.js, Bootstrap), and inline scripts (for charts)
            // - Styles from self, fonts.googleapis.com, cdn.jsdelivr.net, and inline styles
            // - Fonts from self, fonts.gstatic.com, cdn.jsdelivr.net
            // - Images from self and data URIs
            // - AJAX requests to self only
            context.Response.Headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "img-src 'self' https://images.unsplash.com data:; " +
                "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://unpkg.com; " +
                "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://fonts.googleapis.com; " +
                "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net data:; " +
                "connect-src 'self' https://cdn.jsdelivr.net http://localhost:* https://localhost:* ws://localhost:* wss://localhost:*;";


            // Continue to the next middleware in the pipeline
            await _next(context);
        }
    }
}
