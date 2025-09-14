import { signal, WritableSignal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import {
  AppInsightsService,
  AuthService,
  LanguageService,
  LoadingService,
  SignalrService,
  SignalRConnectionStatus,
  ThemeService,
} from '@adventureworks-web/shared/util';
import { AppLayoutComponent } from './app-layout';

describe('AppLayoutComponent', () => {
  let component: AppLayoutComponent;
  let fixture: ComponentFixture<AppLayoutComponent>;
  let connectionStatus: WritableSignal<SignalRConnectionStatus>;

  beforeEach(async () => {
    connectionStatus = signal<SignalRConnectionStatus>('connected');

    await TestBed.configureTestingModule({
      imports: [AppLayoutComponent, TranslateModule.forRoot()],
      providers: [
        provideRouter([]),
        {
          provide: AuthService,
          useValue: {
            isAuthenticated: signal(false),
            user: signal(null),
            displayName: signal(''),
            userInitials: signal(''),
            login: vi.fn(),
            logout: vi.fn(),
            initialize: vi.fn(),
          },
        },
        {
          provide: ThemeService,
          useValue: {
            darkMode: signal(false),
            toggleTheme: vi.fn(),
          },
        },
        {
          provide: LoadingService,
          useValue: {
            isLoading: signal(false),
          },
        },
        {
          provide: LanguageService,
          useValue: {
            currentLang: signal('en'),
            setLanguage: vi.fn(),
          },
        },
        {
          provide: AppInsightsService,
          useValue: {
            trackEvent: vi.fn(),
            trackException: vi.fn(),
            trackPageView: vi.fn(),
          },
        },
        {
          provide: SignalrService,
          useValue: {
            connectionStatus,
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AppLayoutComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('logs out when user clicks sign out', async () => {
    const authService = TestBed.inject(AuthService);

    component['logout']();
    await fixture.whenStable();

    expect(authService.logout).toHaveBeenCalledOnce();
  });

  it('renders current SignalR connection status', () => {
    fixture.detectChanges();

    const statusText = fixture.nativeElement.querySelector('#aw-connection-status-text') as HTMLElement | null;
    expect(statusText?.textContent?.trim()).toBe('Connected');
  });

  it('updates SignalR connection status when signal changes', () => {
    connectionStatus.set('reconnecting');
    fixture.detectChanges();

    const statusText = fixture.nativeElement.querySelector('#aw-connection-status-text') as HTMLElement | null;
    expect(statusText?.textContent?.trim()).toBe('Reconnecting');

    connectionStatus.set('disconnected');
    fixture.detectChanges();

    expect(statusText?.textContent?.trim()).toBe('Disconnected');
  });
});
