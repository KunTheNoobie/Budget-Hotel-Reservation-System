namespace Assignment.Models
{
    /// <summary>
    /// View model used for displaying error pages (404, 500, etc.).
    /// Contains information about the error that occurred, including a request ID
    /// for tracking and debugging purposes.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Unique identifier for the request that caused the error.
        /// Used for tracking and debugging purposes.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Property indicating whether the RequestId should be displayed.
        /// Returns true if RequestId is not null or empty.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
