import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, OnDestroy, input, signal, viewChild } from '@angular/core';

/** Gradient-fade edge affordance for a horizontally-scrollable container, projected via `<ng-content>`. */
@Component({
  selector: 'aw-scroll-indicator',
  templateUrl: './scroll-indicator.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ScrollIndicatorComponent implements AfterViewInit, OnDestroy {
  readonly ariaLabel = input('Scrollable content');

  private readonly scrollContainer = viewChild.required<ElementRef<HTMLDivElement>>('scrollContainer');
  private resizeObserver: ResizeObserver | undefined;

  protected readonly canScrollLeft = signal(false);
  protected readonly canScrollRight = signal(false);

  ngAfterViewInit(): void {
    this.updateScrollState();
    this.resizeObserver = new ResizeObserver(() => this.updateScrollState());
    this.resizeObserver.observe(this.scrollContainer().nativeElement);
  }

  ngOnDestroy(): void {
    this.resizeObserver?.disconnect();
  }

  protected onScroll(): void {
    this.updateScrollState();
  }

  private updateScrollState(): void {
    const el = this.scrollContainer().nativeElement;
    this.canScrollLeft.set(el.scrollLeft > 0);
    this.canScrollRight.set(el.scrollLeft + el.clientWidth < el.scrollWidth - 1);
  }
}
