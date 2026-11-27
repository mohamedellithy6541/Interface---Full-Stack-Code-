export const environment = {
  /** Base URL of the backend API (no trailing slash). */
  apiBaseUrl: 'http://localhost:5019',

  /**
   * OAuth 2.0 Web client ID from Google Cloud Console. It is a public identifier, not a secret.
   * Leave empty to hide the "Sign in with Google" button.
   */
  googleClientId: '',

  /** Shows the seeded demo accounts on the login page. */
  showDemoAccounts: true,
};
