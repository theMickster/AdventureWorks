import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, provideRouter } from '@angular/router';
import { PurchaseOrderDetailPlaceholderComponent } from './purchase-order-detail-placeholder';

function buildRoute(id = '3932') {
  return {
    snapshot: {
      paramMap: { get: vi.fn().mockReturnValue(id) },
    },
  };
}

describe('PurchaseOrderDetailPlaceholderComponent', () => {
  let fixture: ComponentFixture<PurchaseOrderDetailPlaceholderComponent>;
  let router: Router;
  let route: ReturnType<typeof buildRoute>;

  beforeEach(async () => {
    route = buildRoute();

    await TestBed.configureTestingModule({
      imports: [PurchaseOrderDetailPlaceholderComponent],
      providers: [provideRouter([]), { provide: ActivatedRoute, useValue: route }],
    }).compileComponents();

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(PurchaseOrderDetailPlaceholderComponent);
  });

  it('renders the purchase order id from the route', () => {
    fixture.detectChanges();

    const title = fixture.nativeElement.querySelector('#aw-purchase-order-detail-placeholder-title') as HTMLElement;
    expect(title.textContent).toContain('PO3932');
  });

  it('redirects to /purchasing/vendors when the route id is invalid', async () => {
    route = buildRoute('not-a-number');
    TestBed.resetTestingModule();
    await TestBed.configureTestingModule({
      imports: [PurchaseOrderDetailPlaceholderComponent],
      providers: [provideRouter([]), { provide: ActivatedRoute, useValue: route }],
    }).compileComponents();

    const newRouter = TestBed.inject(Router);
    vi.spyOn(newRouter, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(PurchaseOrderDetailPlaceholderComponent);
    fixture.detectChanges();

    expect(newRouter.navigate).toHaveBeenCalledWith(['/purchasing/vendors']);
  });
});
