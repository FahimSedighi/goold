/**
 * Authentication utility functions for managing JWT tokens and user sessions.
 * This module provides a clean interface for authentication operations.
 */

const AuthService = {
    /**
     * Stores the JWT token in localStorage.
     * @param {string} token - The JWT token to store
     */
    setToken: function(token) {
        if (token) {
            localStorage.setItem('authToken', token);
        }
    },

    /**
     * Gets the JWT token from localStorage.
     * @returns {string|null} The JWT token or null if not found
     */
    getToken: function() {
        return localStorage.getItem('authToken');
    },

    /**
     * Removes the JWT token from localStorage.
     */
    clearToken: function() {
        localStorage.removeItem('authToken');
    },

    /**
     * Checks if user is authenticated (has a token).
     * @returns {boolean} True if authenticated, false otherwise
     */
    isAuthenticated: function() {
        return this.getToken() !== null;
    },

    /**
     * Redirects to login page if user is not authenticated.
     * @param {string} loginUrl - The login page URL (default: '/Home')
     */
    requireAuth: function(loginUrl = '/Home') {
        if (!this.isAuthenticated()) {
            window.location.href = loginUrl;
        }
    },

    /**
     * Logs out the user by clearing token and redirecting.
     * @param {string} redirectUrl - The URL to redirect to after logout (default: '/Home')
     */
    logout: async function(redirectUrl = '/Home') {
        try {
            // Call logout API endpoint
            await fetch('/api/auth/logout', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
        } catch (error) {
            console.error('Logout API call failed:', error);
        } finally {
            // Clear token from localStorage
            this.clearToken();
            
            // Delete cookies
            document.cookie = 'AuthToken=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
            document.cookie = 'RefreshToken=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
            
            // Redirect to home page
            window.location.href = redirectUrl;
        }
    }
};

// Export for use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AuthService;
}

