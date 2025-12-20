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


/**
 * Initialize Toast Notifications from Server-Side TempData
 * This function should be called from the layout file after TempData is available.
 * Checks for TempData messages from the server (Success, Error, Info, Warning)
 * and displays them as toast notifications when the page loads.
 * Also handles newsletter-specific messages.
 * 
 * Note: The actual TempData initialization is done in the layout files using Razor syntax.
 * This function is kept here for reference but the initialization code should be in _Layout.cshtml or _AdminLayout.cshtml
 */
function initializeTempDataToasts() {
    // This function is called from layout files with Razor-generated code
    // The actual implementation is in the layout files
}

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
