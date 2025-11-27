using Assignment.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using BookingModel = Assignment.Models.Booking;

namespace Assignment.ViewModels.Booking
{
    /// <summary>
    /// View model for the payment form when completing a booking.
    /// Supports multiple payment methods: Credit Card, PayPal, and Bank Transfer.
    /// Contains validation attributes for each payment method's required fields.
    /// </summary>
    public class PaymentViewModel
    {
        public int BookingId { get; set; }

        [BindNever]
        [ValidateNever]
        public BookingModel? Booking { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        [Display(Name = "Payment Method")]
        public PaymentMethod? PaymentMethod { get; set; }

        // Credit Card Fields
        [Display(Name = "Card Number")]
        [StringLength(19, MinimumLength = 13, ErrorMessage = "Card number must be between 13 and 19 digits")]
        public string? CardNumber { get; set; }

        [Display(Name = "Cardholder Name")]
        [StringLength(100)]
        public string? CardholderName { get; set; }

        [Display(Name = "Expiry Month")]
        [Range(1, 12, ErrorMessage = "Invalid month")]
        public int? ExpiryMonth { get; set; }

        [Display(Name = "Expiry Year")]
        [Range(2024, 2099, ErrorMessage = "Invalid year")]
        public int? ExpiryYear { get; set; }

        [Display(Name = "CVV")]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "CVV must be 3 or 4 digits")]
        public string? CVV { get; set; }

        // PayPal Fields
        [Display(Name = "PayPal Email")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? PayPalEmail { get; set; }

        // Bank Transfer Fields
        [Display(Name = "Bank Name")]
        [StringLength(100)]
        public string? BankName { get; set; }

        [Display(Name = "Account Number")]
        [StringLength(50)]
        public string? AccountNumber { get; set; }

        [Display(Name = "Account Holder Name")]
        [StringLength(100)]
        public string? AccountHolderName { get; set; }

        [Display(Name = "Reference Number")]
        [StringLength(50)]
        public string? ReferenceNumber { get; set; }
    }
}

