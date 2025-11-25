using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Assignment.Services;

namespace Assignment.Models
{
    public enum UserRole { Admin, Manager, Staff, Customer }

    public class User
    {
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [StringLength(255)] // Increased length for encrypted string
        public string? PhoneNumber { get; set; }

        [NotMapped]
        public string? DecryptedPhoneNumber 
        { 
            get => EncryptionService.Decrypt(PhoneNumber ?? "");
            set => PhoneNumber = EncryptionService.Encrypt(value ?? "");
        }

        [Required]
        public UserRole Role { get; set; } = UserRole.Customer;

        public bool IsEmailVerified { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [StringLength(255)]
        public string? ProfilePictureUrl { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        [StringLength(10)]
        public string PreferredLanguage { get; set; } = "en-US";

        [StringLength(50)]
        public string Theme { get; set; } = "Default";

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation Properties
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<FavoriteRoomType> FavoriteRoomTypes { get; set; } = new List<FavoriteRoomType>();
    }
}