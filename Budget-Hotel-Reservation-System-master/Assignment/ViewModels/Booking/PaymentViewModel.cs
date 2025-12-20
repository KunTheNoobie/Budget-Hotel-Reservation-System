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
    /// Used by BookingController.Payment and ProcessPayment actions.
    /// </summary>
    public class PaymentViewModel
    {
        /// <summary>
        /// ID of the booking that payment is being processed for.
        /// Required - links payment to the specific booking.
        /// </summary>
        public int BookingId { get; set; }

        /// <summary>
        /// Full booking object loaded from database.
        /// Used to display booking details on payment page (room, dates, price, etc.).
        /// Not bound from form data (BindNever) - loaded separately in controller.
        /// </summary>
        [BindNever]
        [ValidateNever]
        public BookingModel? Booking { get; set; }

        /// <summary>
        /// Payment method selected by user.
        /// Options: CreditCard (0), PayPal (1), BankTransfer (2).
        /// Required field - user must select a payment method.
        /// Determines which payment fields are required for validation.
        /// </summary>
        [Required(ErrorMessage = "Payment method is required")]
        [Display(Name = "Payment Method")]
        public PaymentMethod? PaymentMethod { get; set; }

        // ========== CREDIT CARD FIELDS ==========
        // These fields are required only if PaymentMethod = CreditCard
        
        /// <summary>
        /// Credit card number (13-19 digits).
        /// Required if PaymentMethod = CreditCard.
        /// Used for promotion abuse prevention (hashed before storage).
        /// Not stored in full - only last 4 digits + hash stored for security.
        /// </summary>
        [Display(Name = "Card Number")]
        [StringLength(19, MinimumLength = 13, ErrorMessage = "Card number must be between 13 and 19 digits")]
        public string? CardNumber { get; set; }

        /// <summary>
        /// Name of the cardholder as printed on the card.
        /// Required if PaymentMethod = CreditCard.
        /// Maximum length: 100 characters.
        /// </summary>
        [Display(Name = "Cardholder Name")]
        [StringLength(100)]
        public string? CardholderName { get; set; }

        /// <summary>
        /// Credit card expiry month (1-12).
        /// Required if PaymentMethod = CreditCard.
        /// Must be valid month number (1 = January, 12 = December).
        /// </summary>
        [Display(Name = "Expiry Month")]
        [Range(1, 12, ErrorMessage = "Invalid month")]
        public int? ExpiryMonth { get; set; }

        /// <summary>
        /// Credit card expiry year (2024-2099).
        /// Required if PaymentMethod = CreditCard.
        /// Must be current year or future year.
        /// </summary>
        [Display(Name = "Expiry Year")]
        [Range(2024, 2099, ErrorMessage = "Invalid year")]
        public int? ExpiryYear { get; set; }

        /// <summary>
        /// Card Verification Value (CVV) - 3 or 4 digit security code.
        /// Required if PaymentMethod = CreditCard.
        /// Located on back of card (Visa/Mastercard) or front (American Express).
        /// </summary>
        [Display(Name = "CVV")]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "CVV must be 3 or 4 digits")]
        public string? CVV { get; set; }

        // ========== PAYPAL FIELDS ==========
        // These fields are required only if PaymentMethod = PayPal
        
        /// <summary>
        /// PayPal email address associated with the PayPal account.
        /// Required if PaymentMethod = PayPal.
        /// Must be valid email format.
        /// Used to process PayPal payment.
        /// </summary>
        [Display(Name = "PayPal Email")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? PayPalEmail { get; set; }

        // ========== BANK TRANSFER FIELDS ==========
        // These fields are required only if PaymentMethod = BankTransfer
        
        /// <summary>
        /// Name of the bank for bank transfer payment.
        /// Required if PaymentMethod = BankTransfer.
        /// Maximum length: 100 characters.
        /// </summary>
        [Display(Name = "Bank Name")]
        [StringLength(100)]
        public string? BankName { get; set; }

        /// <summary>
        /// Bank account number for transfer.
        /// Required if PaymentMethod = BankTransfer.
        /// Maximum length: 50 characters.
        /// </summary>
        [Display(Name = "Account Number")]
        [StringLength(50)]
        public string? AccountNumber { get; set; }

        /// <summary>
        /// Name of the account holder (must match bank records).
        /// Required if PaymentMethod = BankTransfer.
        /// Maximum length: 100 characters.
        /// </summary>
        [Display(Name = "Account Holder Name")]
        [StringLength(100)]
        public string? AccountHolderName { get; set; }

        /// <summary>
        /// Reference number for the bank transfer (optional).
        /// Used to track the transfer transaction.
        /// Maximum length: 50 characters.
        /// </summary>
        [Display(Name = "Reference Number")]
        [StringLength(50)]
        public string? ReferenceNumber { get; set; }
    }
}

