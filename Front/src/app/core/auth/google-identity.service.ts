import { Injectable } from '@angular/core';

/** The small part of the Google Identity Services API we use. */
interface GoogleIdentity {
  accounts: {
    id: {
      initialize(config: { client_id: string; callback: (response: { credential: string }) => void }): void;
      renderButton(parent: HTMLElement, options: Record<string, string | number>): void;
    };
  };
}

declare global {
  interface Window {
    google?: GoogleIdentity;
  }
}

const SCRIPT_URL = 'https://accounts.google.com/gsi/client';

/**
 * Loads Google's sign-in script once, on demand. We use Google's script directly instead of an
 * npm wrapper: it is two calls, and the script must come from Google anyway.
 */
@Injectable({ providedIn: 'root' })
export class GoogleIdentityService {
  private loading?: Promise<GoogleIdentity>;

  load(): Promise<GoogleIdentity> {
    this.loading ??= new Promise<GoogleIdentity>((resolve, reject) => {
      if (window.google) {
        resolve(window.google);
        return;
      }
      const script = document.createElement('script');
      script.src = SCRIPT_URL;
      script.async = true;
      script.defer = true;
      script.onload = () => (window.google ? resolve(window.google) : reject(new Error('Google script did not load.')));
      script.onerror = () => {
        this.loading = undefined;
        reject(new Error('Google script could not be loaded.'));
      };
      document.head.appendChild(script);
    });
    return this.loading;
  }
}
