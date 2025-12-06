import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AuthService, ThemeService } from '@adventureworks-web/shared/util';
import { PublicNavComponent } from './public-nav';

describe('PublicNavComponent', () => {
  let fixture: ComponentFixture<PublicNavComponent>;
  let login: ReturnType<typeof vi.fn>;
  let logout: ReturnType<typeof vi.fn>;
  let isAuthenticated: ReturnType<typeof signal<boolean>>;

  beforeEach(async () => {
    login = vi.fn();
    logout = vi.fn();
    isAuthenticated = signal(false);

    await TestBed.configureTestingModule({
      imports: [PublicNavComponent],
      providers: [
        provideRouter([]),
        {
          provide: AuthService,
          useValue: {
            isAuthenticated,
            user: signal(null),
            displayName: signal(''),
            userInitials: signal(''),
            login,
            logout,
            initialize: vi.fn(),
          },
        },
        {
          provide: ThemeService,
          useValue: {
            darkMode: signal(false),
            toggle: vi.fn(),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PublicNavComponent);
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('triggers login when the Log In button is clicked', () => {
    fixture.detectChanges();

    const loginBtn = fixture.nativeElement.querySelector('#aw-public-login-btn') as HTMLButtonElement;
    loginBtn.click();

    expect(login).toHaveBeenCalledOnce();
  });

  it('shows Dashboard and Sign Out buttons instead of Log In when authenticated', () => {
    isAuthenticated.set(true);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('#aw-public-login-btn')).toBeNull();
    expect(fixture.nativeElement.querySelector('#aw-public-dashboard-btn')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('#aw-public-logout-btn')).toBeTruthy();
  });

  it('triggers logout when the Sign Out button is clicked', () => {
    isAuthenticated.set(true);
    fixture.detectChanges();

    const logoutBtn = fixture.nativeElement.querySelector('#aw-public-logout-btn') as HTMLButtonElement;
    logoutBtn.click();

    expect(logout).toHaveBeenCalledOnce();
  });
});
