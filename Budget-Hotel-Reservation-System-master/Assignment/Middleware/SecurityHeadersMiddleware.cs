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
        /// These headers are added to every HTTP response to protect against common web vulnerabilities.
        /// </summary>
        /// <param name="context">The HTTP context for the current request.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            // ========== X-FRAME-OPTIONS HEADER ==========
            // Prevent the page from being displayed in an iframe (clickjacking protection)
            // Clickjacking: Attackers embed your site in an iframe and trick users into clicking buttons
            // "DENY" = Never allow this page to be displayed in a frame
            // Alternative: "SAMEORIGIN" = Allow only from same origin
            context.Response.Headers["X-Frame-Options"] = "DENY";

            // ========== X-XSS-PROTECTION HEADER ==========
            // Enable browser's built-in XSS (Cross-Site Scripting) protection
            // "1; mode=block" = Enable protection and block the page if XSS is detected
            // This provides an additional layer of protection against XSS attacks
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

            // ========== X-CONTENT-TYPE-OPTIONS HEADER ==========
            // Prevent MIME type sniffing (prevents browsers from guessing content types)
            // MIME sniffing: Browser tries to guess file type even if server says it's text/plain
            // This can lead to security vulnerabilities (e.g., executing JavaScript from text file)
            // "nosniff" = Browser must use the Content-Type header provided by server
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";

            // ========== REFERRER-POLICY HEADER ==========
            // Control how much referrer information is sent with requests
            // Referrer: The URL of the page that linked to this page
            // "strict-origin-when-cross-origin" = Send full referrer for same-origin, only origin for cross-origin
            // This protects user privacy while maintaining functionality
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // ========== CONTENT-SECURITY-POLICY HEADER ==========
            // Content-Security-Policy: Restricts which resources can be loaded
            // This is a powerful security feature that prevents XSS attacks by controlling resource loading
            // Permissive policy for assignment to avoid breaking scripts/styles
            // Allows:
            // - Scripts from self, cdn.jsdelivr.net (Chart.js, Bootstrap), and inline scripts (for charts)
            // - Styles from self, fonts.googleapis.com, cdn.jsdelivr.net, and inline styles
            // - Fonts from self, fonts.gstatic.com, cdn.jsdelivr.net
            // - Images from self, images.unsplash.com, and data URIs
            // - AJAX requests to self, cdn.jsdelivr.net, and localhost (for development)
            context.Response.Headers["Content-Security-Policy"] =
                "default-src 'self'; " +                                                    // Default: Only allow resources from same origin
                "img-src 'self' https://images.unsplash.com data:; " +                     // Images: Allow from self, Unsplash, and data URIs
                "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://unpkg.com; " +  // Scripts: Allow from self, CDNs, and inline (for charts)
                "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://fonts.googleapis.com; " +  // Styles: Allow from self, CDNs, and inline
                "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net data:; " +  // Fonts: Allow from self, Google Fonts, CDNs, and data URIs
                "connect-src 'self' https://cdn.jsdelivr.net http://localhost:* https://localhost:* ws://localhost:* wss://localhost:*;";  // AJAX: Allow to self, CDNs, and localhost (for development)

            // ========== CONTINUE TO NEXT MIDDLEWARE ==========
            // Continue to the next middleware in the pipeline
            // This allows the request to continue processing after security headers are added
            await _next(context);
        }
    }
}
