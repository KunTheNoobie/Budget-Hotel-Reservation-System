/**
 * Budget Hotel Reservation System - Client-Side JavaScript
 * 
 * This file contains global JavaScript functions for:
 * - AJAX loading indicators
 * - Toast notification system
 * - Image preview functionality
 * 
 * Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
 * for details on configuring this project to bundle and minify static web assets.
 */

/**
 * Global AJAX Loading Indicator
 * Shows/hides a loading spinner during AJAX requests to provide user feedback.
 * Automatically displays when any AJAX request starts and hides when it completes or errors.
 */
$(document).ajaxStart(function() {
    $('#loadingIndicator').fadeIn(200);
}).ajaxStop(function() {
    $('#loadingIndicator').fadeOut(200);
}).ajaxError(function(event, xhr, settings, thrownError) {
    $('#loadingIndicator').fadeOut(200);
});

/**
 * Toast Notification System
 * Displays temporary notification messages to users (success, error, warning, info).
 * Uses Bootstrap's toast component for styling and animation.
 * 
 * @param {string} message - The message to display in the toast notification
 * @param {string} type - The type of toast: 'success', 'error', 'warning', or 'info' (default: 'info')
 * @example
 * showToast('Booking confirmed!', 'success');
 * showToast('An error occurred', 'error');
 */
function showToast(message, type = 'info') {
    const toastId = 'toast-' + Date.now();
    const bgColor = {
        'success': 'bg-success',
        'error': 'bg-danger',
        'warning': 'bg-warning',
        'info': 'bg-info'
    }[type] || 'bg-info';
    
    const icon = {
        'success': 'bi-check-circle-fill',
        'error': 'bi-exclamation-circle-fill',
        'warning': 'bi-exclamation-triangle-fill',
        'info': 'bi-info-circle-fill'
    }[type] || 'bi-info-circle-fill';
    
    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center text-white ${bgColor} border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    <i class="bi ${icon} me-2"></i>${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;
    
    $('#toastContainer').append(toastHtml);
    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement, {
        autohide: true,
        delay: 5000
    });
    toast.show();
    
    // Remove toast element after it's hidden
    toastElement.addEventListener('hidden.bs.toast', function() {
        $(this).remove();
    });
}

/**
 * Initialize Toast Notifications from Server-Side TempData
 * Checks for TempData messages from the server (Success, Error, Info, Warning)
 * and displays them as toast notifications when the page loads.
 * Also handles newsletter-specific messages.
 */
$(document).ready(function() {
    // Check for TempData messages from server-side (ASP.NET Core TempData)
    @if (TempData["Success"] != null)
    {
        <text>showToast('@Html.Raw(TempData["Success"])', 'success');</text>
    }
    @if (TempData["Error"] != null)
    {
        <text>showToast('@Html.Raw(TempData["Error"])', 'error');</text>
    }
    @if (TempData["Info"] != null)
    {
        <text>showToast('@Html.Raw(TempData["Info"])', 'info');</text>
    }
    @if (TempData["Warning"] != null)
    {
        <text>showToast('@Html.Raw(TempData["Warning"])', 'warning');</text>
    }
    
    // Check for newsletter errors
    @if (TempData["NewsletterError"] != null)
    {
        <text>showToast('@Html.Raw(TempData["NewsletterError"])', 'error');</text>
    }
    @if (TempData["NewsletterSuccess"] != null)
    {
        <text>showToast('@Html.Raw(TempData["NewsletterSuccess"])', 'success');</text>
    }
});

/**
 * Image Preview Function
 * Displays a preview of an image file before it is uploaded.
 * Used for profile picture uploads and other image selection scenarios.
 * 
 * @param {HTMLInputElement} input - The file input element containing the selected file
 * @param {string} previewId - The ID of the img element where the preview should be displayed
 * @example
 * previewImage(document.getElementById('fileInput'), 'imagePreview');
 */
function previewImage(input, previewId) {
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function(e) {
            $('#' + previewId).attr('src', e.target.result).show();
            $('#' + previewId).parent().show();
        };
        reader.readAsDataURL(input.files[0]);
    }
}
