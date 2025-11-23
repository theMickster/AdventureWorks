import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { OrderDetailComponent } from './order-detail';

function buildRoute(id: string) {
  return {
    snapshot: { paramMap: { get: vi.fn().mockReturnValue(id) } },
  };
}

describe('OrderDetailComponent', () => {
  let fixture: ComponentFixture<OrderDetailComponent>;

  async function setup(id: string): Promise<void> {
    await TestBed.configureTestingModule({
      imports: [OrderDetailComponent],
      providers: [provideRouter([]), { provide: ActivatedRoute, useValue: buildRoute(id) }],
    }).compileComponents();

    fixture = TestBed.createComponent(OrderDetailComponent);
  }

  it('renders the component', async () => {
    await setup('43659');
    fixture.detectChanges();
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('reads the sales order id from the route snapshot', async () => {
    await setup('43659');
    fixture.detectChanges();

    expect(fixture.componentInstance['salesOrderId']()).toBe(43659);
    const heading = fixture.nativeElement.querySelector('h1') as HTMLElement;
    expect(heading.textContent).toContain('43659');
  });

  it('shows the placeholder and a back link to the order list', async () => {
    await setup('1');
    fixture.detectChanges();

    const placeholder = fixture.nativeElement.querySelector('#aw-order-detail-placeholder') as HTMLElement;
    expect(placeholder.textContent).toContain('coming soon');

    const backLink = fixture.nativeElement.querySelector('#aw-order-detail-back-btn') as HTMLAnchorElement;
    expect(backLink).toBeTruthy();
    expect(backLink.getAttribute('href')).toBe('/sales/orders');
  });
});
