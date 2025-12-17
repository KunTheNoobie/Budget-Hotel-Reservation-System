using System.ComponentModel.DataAnnotations;

namespace Assignment.Models
{
    /// <summary>
    /// Represents a contact message submitted through the contact form on the website.
    /// Used for customer inquiries, feedback, and support requests.
    /// </summary>
    public class ContactMessage
    {
        /// <summary>
        /// Primary key identifier for the contact message.
        /// </summary>
        [Key]
        public int MessageId { get; set; }

        /// <summary>
        /// Name of the person who submitted the contact message.
        /// Required field with maximum length of 100 characters.
        /// </summary>
        [Required, StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Email address of the person who submitted the contact message.
        /// Required field, must be a valid email format, maximum length of 100 characters.
        /// </summary>
        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; }

        /// <summary>
        /// Subject line of the contact message.
        /// Required field with maximum length of 200 characters.
        /// </summary>
        [Required, StringLength(200)]
        public string Subject { get; set; }

        /// <summary>
        /// Content/body of the contact message.
        /// Required field with maximum length of 2000 characters.
        /// </summary>
        [Required, StringLength(2000)]
        public string Message { get; set; }

        /// <summary>
        /// Date and time when the contact message was submitted.
        /// Defaults to the current date/time.
        /// </summary>
        public DateTime SentAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Flag indicating whether an administrator has read the message.
        /// Used for tracking unread messages in the admin panel.
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// Soft delete flag - indicates if the contact message has been deleted.
        /// When true, the message is hidden from queries but not physically removed from the database.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp of when the contact message was soft deleted.
        /// Null if the message has not been deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }
    }
}
