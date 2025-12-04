import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AuthService, ThemeService } from '@adventureworks-web/shared/util';
import { PublicNavComponent } from './public-nav';

describe('PublicNavComponent', () => {
  let fixture: ComponentFixture<PublicNavComponent>;
  let login: ReturnType<typeof vi.fn>;

  beforeEach(async () => {
    login = vi.fn();

    await TestBed.configureTestingModule({
      imports: [PublicNavComponent],
      providers: [
        provideRouter([]),
        {
          provide: AuthService,
          useValue: {
            isAuthenticated: signal(false),
            user: signal(null),
            displayName: signal(''),
            userInitials: signal(''),
            login,
            logout: vi.fn(),
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
});
