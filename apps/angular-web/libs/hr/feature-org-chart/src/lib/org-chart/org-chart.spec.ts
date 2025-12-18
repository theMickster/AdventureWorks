import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { By } from '@angular/platform-browser';
import { patchState } from '@ngrx/signals';
import { unprotected } from '@ngrx/signals/testing';
import { ENVIRONMENT, NotificationService } from '@adventureworks-web/shared/util';
import { setError, setLoaded, setLoading } from '@adventureworks-web/shared/data-access';
import type { EmployeeOrgTreeItem } from '@adventureworks-web/hr/data-access';
import { OrgChartStore } from '../stores/org-chart.store';
import { OrgChartComponent } from './org-chart';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

const mockItems: EmployeeOrgTreeItem[] = [
  { employeeId: 1, fullName: 'Ken Sánchez', jobTitle: 'Chief Executive Officer', departmentName: 'Executive', organizationLevel: null, parentEmployeeId: null },
  { employeeId: 273, fullName: 'Brian Welcker', jobTitle: 'Vice President of Sales', departmentName: 'Sales', organizationLevel: 1, parentEmployeeId: null },
];

describe('OrgChartComponent', () => {
  let fixture: ComponentFixture<OrgChartComponent>;
  let store: InstanceType<typeof OrgChartStore>;
  let notificationService: NotificationService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrgChartComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
      ],
    }).compileComponents();

    store = TestBed.inject(OrgChartStore);
    notificationService = TestBed.inject(NotificationService);
    vi.spyOn(store, 'load');

    fixture = TestBed.createComponent(OrgChartComponent);
  });

  function loadWithItems(): void {
    patchState(unprotected(store), { items: mockItems, departmentColorClasses: { Executive: 'primary', Sales: 'accent' } }, setLoaded());
  }

  it('calls store.load() once on ngOnInit', () => {
    fixture.detectChanges();
    expect(store.load).toHaveBeenCalledTimes(1);
  });

  it('shows skeleton stats while isLoading', () => {
    patchState(unprotected(store), setLoading());
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelectorAll('aw-skeleton').length).toBeGreaterThan(0);
  });

  it('renders the stats bar reflecting store.stats() once loaded', () => {
    fixture.detectChanges();
    loadWithItems();
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;
    expect(fixture.nativeElement.querySelector('#aw-org-chart-stat-total').textContent).toContain('2');
    expect(text).toContain('Total Employees');
  });

  it('renders the root aw-org-node once loaded', () => {
    fixture.detectChanges();
    loadWithItems();
    fixture.detectChanges();

    const rootNode = fixture.debugElement.query(By.css('aw-org-node'));
    expect(rootNode).toBeTruthy();
  });

  it('debounces search input and invokes store.searchAndExpand', async () => {
    vi.useFakeTimers();
    fixture.detectChanges();
    loadWithItems();
    fixture.detectChanges();
    vi.spyOn(store, 'searchAndExpand');

    const input = fixture.nativeElement.querySelector('#aw-org-chart-search-input') as HTMLInputElement;
    input.value = 'Brian';
    input.dispatchEvent(new Event('input'));

    expect(store.searchAndExpand).not.toHaveBeenCalled();

    vi.advanceTimersByTime(300);

    expect(store.searchAndExpand).toHaveBeenCalledWith('Brian');
    vi.useRealTimers();
  });

  it('navigates to the employee detail route when a node emits navigate', () => {
    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture.detectChanges();
    loadWithItems();
    fixture.detectChanges();

    const rootNode = fixture.debugElement.query(By.css('aw-org-node'));
    rootNode.componentInstance.navigate.emit(273);

    expect(router.navigate).toHaveBeenCalledWith(['/hr/employees', 273]);
  });

  it('fires an error notification when hasError becomes true', () => {
    vi.spyOn(notificationService, 'error');
    fixture.detectChanges();

    patchState(unprotected(store), setError('Failed to load organization chart'));
    fixture.detectChanges();

    expect(notificationService.error).toHaveBeenCalledWith('Failed to load the organization chart. Please try again.');
  });
});
