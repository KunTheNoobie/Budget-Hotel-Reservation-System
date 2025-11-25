namespace Assignment.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            
            // Content-Security-Policy (Permissive for assignment to avoid breaking scripts/styles)
            // Allows scripts from self, cdn.jsdelivr.net (Chart.js, Bootstrap), and inline scripts (for our charts)
            // Allows styles from self, fonts.googleapis.com, cdn.jsdelivr.net
            // Allows fonts from self, fonts.gstatic.com, cdn.jsdelivr.net
            context.Response.Headers["Content-Security-Policy"] = 
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
                "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
                "img-src 'self' data:; " +
                "connect-src 'self';";

            await _next(context);
        }
    }
}
