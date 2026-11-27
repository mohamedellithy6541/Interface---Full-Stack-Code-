import { ChangeDetectionStrategy, Component, ElementRef, afterNextRender, inject, input, output, signal, viewChild } from '@angular/core';

import { GoogleIdentityService } from '../../core/auth/google-identity.service';

/** Renders Google's own "Sign in with Google" button and emits the ID token Google returns. */
@Component({
  selector: 'app-google-sign-in-button',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div #host class="google-button"></div>
    @if (unavailable()) {
      <p class="muted">Google sign-in is unavailable right now.</p>
    }
  `,
  styles: `.google-button { display: flex; justify-content: center; min-height: 44px; }`,
})
export class GoogleSignInButton {
  readonly clientId = input.required<string>();
  readonly credential = output<string>();

  protected readonly unavailable = signal(false);
  private readonly host = viewChild.required<ElementRef<HTMLElement>>('host');
  private readonly googleIdentity = inject(GoogleIdentityService);

  constructor() {
    afterNextRender(() => {
      this.googleIdentity
        .load()
        .then((google) => {
          google.accounts.id.initialize({
            client_id: this.clientId(),
            callback: (response) => this.credential.emit(response.credential),
          });
          google.accounts.id.renderButton(this.host().nativeElement, {
            theme: 'outline',
            size: 'large',
            text: 'signin_with',
            width: 320,
          });
        })
        .catch(() => this.unavailable.set(true));
    });
  }
}
