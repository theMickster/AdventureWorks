import { computed, inject, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { InteractionStatus, RedirectRequest } from '@azure/msal-browser';
import { MSAL_GUARD_CONFIG, MsalBroadcastService, MsalGuardConfiguration, MsalService } from '@azure/msal-angular';
import { filter } from 'rxjs';
import { AuthUser } from './auth.model';

/** Signal-based wrapper around MSAL services for authentication state management. */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly msalService = inject(MsalService);
  private readonly msalBroadcastService = inject(MsalBroadcastService);
  private readonly guardConfig = inject<MsalGuardConfiguration>(MSAL_GUARD_CONFIG);

  readonly isAuthenticated = signal(false);
  readonly user = signal<AuthUser | null>(null);

  readonly displayName = computed(() => this.user()?.name ?? '');
  readonly userInitials = computed(() => {
    const name = this.user()?.name;
    if (!name) return '';
    const parts = name.trim().split(/\s+/);
    const first = parts[0]?.[0] ?? '';
    const last = parts.length > 1 ? parts[parts.length - 1][0] : '';
    return (first + last).toUpperCase();
  });

  constructor() {
    this.msalBroadcastService.inProgress$
      .pipe(
        filter((status) => status === InteractionStatus.None),
        takeUntilDestroyed(),
      )
      .subscribe(() => {
        this.checkAndSetActiveAccount();
        this.updateAuthState();
      });
  }

  /** Call once at app startup to handle redirect responses. */
  initialize(): void {
    this.msalService.handleRedirectObservable().subscribe();
  }

  /** Trigger a redirect login flow. */
  login(): void {
    const authRequest = this.guardConfig.authRequest;
    this.msalService.loginRedirect({ ...authRequest } as RedirectRequest).subscribe();
  }

  /** Trigger a redirect logout flow. */
  logout(): void {
    this.msalService.logoutRedirect().subscribe();
  }

  private checkAndSetActiveAccount(): void {
    const activeAccount = this.msalService.instance.getActiveAccount();
    if (!activeAccount) {
      const accounts = this.msalService.instance.getAllAccounts();
      if (accounts.length > 0) {
        this.msalService.instance.setActiveAccount(accounts[0]);
      }
    }
  }

  private updateAuthState(): void {
    const accounts = this.msalService.instance.getAllAccounts();
    this.isAuthenticated.set(accounts.length > 0);

    const active = this.msalService.instance.getActiveAccount();
    if (active) {
      this.user.set({
        name: active.name ?? '',
        email: active.username,
        oid: active.localAccountId,
        username: active.username,
      });
    } else {
      this.user.set(null);
    }
  }
}
