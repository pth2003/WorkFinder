/**
 * Utils.js - Utility functions for WorkFinder application
 */

/**
 * Debounce function to limit how often a function is called
 * @param {Function} func - The function to debounce
 * @param {number} wait - The time to wait in milliseconds
 * @returns {Function} - Debounced function
 */
function debounce(func, wait) {
    let timeout;
    
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

/**
 * Converts form data to URL query string
 * @param {HTMLFormElement} form - The form element
 * @returns {string} - URL encoded query string
 */
function getFormDataAsQueryString(form) {
    const formData = new FormData(form);
    return new URLSearchParams(formData).toString();
} 