import { ChangeDetectionStrategy, Component, DestroyRef, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { CurrentUserDto } from '../../core/api/generated/models';
import { SessionService } from '../../core/auth/session.service';
import { toApiError } from '../../core/http/api-error';
import { FieldError } from '../../shared/forms/field-error';
import { GoogleSignInButton } from './google-sign-in-button';

@Component({
  selector: 'app-login-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, FieldError, GoogleSignInButton],
  templateUrl: './login-page.html',
  styleUrl: './login-page.scss',
})
export class LoginPage {
  /** Query parameter set by the auth guard: where to go after signing in. */
  readonly returnUrl = input<string>();

  private readonly session = inject(SessionService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly googleClientId = environment.googleClientId;
  protected readonly showDemoAccounts = environment.showDemoAccounts;
  protected readonly signingIn = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly form = new FormGroup({
    email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    password: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { email, password } = this.form.getRawValue();
    this.signIn(this.session.login(email, password));
  }

  protected signInWithGoogle(idToken: string): void {
    this.signIn(this.session.loginWithGoogle(idToken));
  }

  protected useDemoAccount(email: string): void {
    this.form.setValue({ email, password: 'Demo@12345' });
  }

  private signIn(request: Observable<CurrentUserDto>): void {
    this.error.set(null);
    this.signingIn.set(true);
    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => void this.router.navigateByUrl(this.safeReturnUrl() ?? this.session.homeUrl()),
      error: (error: unknown) => {
        this.error.set(toApiError(error).message);
        this.signingIn.set(false);
      },
    });
  }

  /** Only follow in-app paths, never a full URL that could send the user to another site. */
  private safeReturnUrl(): string | null {
    const url = this.returnUrl();
    return url && url.startsWith('/') && !url.startsWith('//') && url !== '/login' ? url : null;
  }
}
