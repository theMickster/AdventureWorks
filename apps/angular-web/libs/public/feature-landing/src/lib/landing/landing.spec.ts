import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { AuthService } from '@adventureworks-web/shared/util';
import { LandingComponent } from './landing';

describe('LandingComponent', () => {
  let fixture: ComponentFixture<LandingComponent>;
  let isAuthenticated: ReturnType<typeof signal<boolean>>;
  let router: Router;

  beforeEach(async () => {
    isAuthenticated = signal(false);

    await TestBed.configureTestingModule({
      imports: [LandingComponent],
      providers: [
        provideRouter([]),
        {
          provide: AuthService,
          useValue: {
            isAuthenticated,
            user: signal(null),
            displayName: signal(''),
            userInitials: signal(''),
            login: vi.fn(),
            logout: vi.fn(),
            initialize: vi.fn(),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LandingComponent);
    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('does not redirect an unauthenticated visitor', () => {
    fixture.detectChanges();

    expect(router.navigateByUrl).not.toHaveBeenCalled();
  });

  it('redirects to the dashboard once the visitor is authenticated', async () => {
    fixture.detectChanges();

    isAuthenticated.set(true);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(router.navigateByUrl).toHaveBeenCalledWith('/dashboard');
  });
});
