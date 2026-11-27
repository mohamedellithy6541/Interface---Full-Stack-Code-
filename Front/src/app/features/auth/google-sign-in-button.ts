import { ChangeDetectionStrategy, Component, ElementRef, afterNextRender, inject, input, output, signal, viewChild } from '@angular/core';

import { GoogleIdentityService } from '../../core/auth/google-identity.service';

/**
 * "Sign in with Google". With a client ID it renders Google's own button and emits the ID token Google returns.
 * Without one (not configured yet) it shows a look-alike button that explains what is missing.
 */
@Component({
  selector: 'app-google-sign-in-button',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './google-sign-in-button.html',
  styleUrl: './google-sign-in-button.scss',
})
export class GoogleSignInButton {
  readonly clientId = input('');
  readonly credential = output<string>();

  protected readonly unavailable = signal(false);
  protected readonly showSetupHint = signal(false);
  private readonly host = viewChild<ElementRef<HTMLElement>>('host');
  private readonly googleIdentity = inject(GoogleIdentityService);

  constructor() {
    afterNextRender(() => {
      const host = this.host()?.nativeElement;
      if (!this.clientId() || !host) return;

      this.googleIdentity
        .load()
        .then((google) => {
          google.accounts.id.initialize({
            client_id: this.clientId(),
            callback: (response) => this.credential.emit(response.credential),
          });
          google.accounts.id.renderButton(host, {
            theme: 'outline',
            size: 'large',
            text: 'signin_with',
            width: 320,
            locale: 'en', // match the rest of the UI instead of the browser language
          });
        })
        .catch(() => this.unavailable.set(true));
    });
  }
}
