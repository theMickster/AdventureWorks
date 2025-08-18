import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService, ThemeService, LoadingService, LanguageService, AppInsightsService } from '@adventureworks-web/shared/util';
import { AppLayoutComponent } from './app-layout';

describe('AppLayoutComponent', () => {
  let component: AppLayoutComponent;
  let fixture: ComponentFixture<AppLayoutComponent>;

  beforeEach(async () => {
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
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AppLayoutComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
