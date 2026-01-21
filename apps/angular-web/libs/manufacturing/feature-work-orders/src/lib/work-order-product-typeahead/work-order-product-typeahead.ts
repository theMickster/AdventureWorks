import { ChangeDetectionStrategy, Component, computed, forwardRef, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Subject, catchError, debounceTime, map, of, switchMap } from 'rxjs';
import { WorkOrderApiService } from '@adventureworks-web/manufacturing/data-access';
import type { ProductLookup } from '@adventureworks-web/manufacturing/data-access';
import { createControlValueAccessor } from '@adventureworks-web/shared/util';

const SEARCH_DEBOUNCE_MS = 300;
/** Mirrors the API's `[MaxLength(50)]` on the product search `name`; a longer term is rejected with a 400. */
const MAX_NAME_LENGTH = 50;

interface SearchOutcome {
  readonly products: ProductLookup[];
  readonly failed: boolean;
}

const NO_RESULTS: SearchOutcome = { products: [], failed: false };
const SEARCH_FAILED: SearchOutcome = { products: [], failed: true };

@Component({
  selector: 'aw-work-order-product-typeahead',
  standalone: true,
  templateUrl: './work-order-product-typeahead.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => WorkOrderProductTypeaheadComponent),
      multi: true,
    },
  ],
})
/**
 * Product name typeahead for the work order list's filter bar (ARIA combobox + listbox).
 *
 * The form value is the selected product id as a string (`''` when nothing is selected), so it
 * drops into the same string-based filter form and URL round-trip the other filters use. Typing
 * clears any prior selection: the box text and the filter must never disagree. A search term only
 * becomes a filter once a product is picked, so unselected text is not applied.
 *
 * A `productId` restored from the URL (e.g. from a scorecard link) is labelled by fetching the
 * product by id. If that lookup fails the box shows `Product #<id>` — the filter still applies
 * and the list is unaffected.
 */
export class WorkOrderProductTypeaheadComponent implements ControlValueAccessor {
  private readonly workOrderApi = inject(WorkOrderApiService);

  readonly fieldId = input('aw-work-order-product-typeahead');
  readonly label = input('Product');

  protected readonly cva = createControlValueAccessor<string>('');
  protected readonly maxNameLength = MAX_NAME_LENGTH;

  protected readonly inputText = signal('');
  protected readonly results = signal<ProductLookup[]>([]);
  protected readonly isOpen = signal(false);
  protected readonly activeIndex = signal(-1);
  /** True once a search for the current text has returned; distinguishes "no match" from "still typing". */
  protected readonly searchDone = signal(false);
  protected readonly searchFailed = signal(false);

  protected readonly listboxId = computed(() => `${this.fieldId()}-listbox`);
  protected readonly showList = computed(() => this.isOpen() && this.results().length > 0);
  protected readonly activeDescendant = computed(() =>
    this.showList() && this.activeIndex() >= 0 ? this.optionId(this.activeIndex()) : null,
  );
  protected readonly statusMessage = computed(() => {
    if (!this.isOpen() || this.inputText().trim() === '') {
      return '';
    }
    if (this.searchFailed()) {
      return 'Product search is unavailable.';
    }
    return this.searchDone() && this.results().length === 0 ? 'No products found.' : '';
  });

  private readonly selected = signal<ProductLookup | null>(null);
  private readonly searchTerm$ = new Subject<string>();
  private readonly resolveId$ = new Subject<number>();

  constructor() {
    // switchMap drops a stale in-flight search when the user keeps typing.
    this.searchTerm$
      .pipe(
        debounceTime(SEARCH_DEBOUNCE_MS),
        switchMap((term) =>
          term === ''
            ? of(NO_RESULTS)
            : this.workOrderApi.searchProducts(term).pipe(
                map((result): SearchOutcome => ({ products: result.results ?? [], failed: false })),
                catchError(() => of(SEARCH_FAILED)),
              ),
        ),
        takeUntilDestroyed(),
      )
      .subscribe((outcome) => {
        this.results.set(outcome.products);
        this.searchFailed.set(outcome.failed);
        this.searchDone.set(true);
        this.activeIndex.set(-1);
      });

    this.resolveId$
      .pipe(
        switchMap((id) =>
          this.workOrderApi.getProduct(id).pipe(
            catchError(() => of<ProductLookup>({ id, name: this.fallbackLabel(id) })),
          ),
        ),
        takeUntilDestroyed(),
      )
      .subscribe((product) => {
        // Ignore a response that lands after the value moved on (cleared, or another product picked).
        if (Number(this.cva.value()) === product.id) {
          this.selected.set(product);
          this.inputText.set(product.name);
        }
      });
  }

  writeValue(val: string | null): void {
    const raw = val ?? '';
    this.cva.writeValue(raw);
    this.isOpen.set(false);

    // Never send an unvalidated URL value to the API.
    const id = Number(raw);
    if (raw === '' || !Number.isInteger(id) || id <= 0) {
      this.selected.set(null);
      this.inputText.set('');
      return;
    }

    const current = this.selected();
    if (current?.id === id) {
      this.inputText.set(current.name);
      return;
    }

    this.inputText.set(this.fallbackLabel(id));
    this.resolveId$.next(id);
  }

  registerOnChange(fn: (val: string) => void): void {
    this.cva.registerOnChange(fn);
  }

  registerOnTouched(fn: () => void): void {
    this.cva.registerOnTouched(fn);
  }

  setDisabledState(isDisabled: boolean): void {
    this.cva.setDisabledState(isDisabled);
  }

  protected optionId(index: number): string {
    return `${this.fieldId()}-option-${index}`;
  }

  protected onInput(event: Event): void {
    const text = (event.target as HTMLInputElement).value;
    this.inputText.set(text);

    if (this.cva.value() !== '') {
      this.selected.set(null);
      this.cva.emitChange('');
    }

    const term = text.trim();
    this.searchDone.set(false);
    this.isOpen.set(term !== '');
    this.searchTerm$.next(term);
  }

  protected onKeydown(event: KeyboardEvent): void {
    const count = this.results().length;

    switch (event.key) {
      case 'ArrowDown':
      case 'ArrowUp': {
        if (count === 0) {
          return;
        }
        event.preventDefault();
        this.isOpen.set(true);
        const step = event.key === 'ArrowDown' ? 1 : -1;
        this.activeIndex.set((this.activeIndex() + step + count) % count);
        return;
      }
      case 'Enter': {
        // Only swallow Enter when it picks an option; otherwise it submits the filter form as usual.
        const product = this.showList() ? this.results()[this.activeIndex()] : undefined;
        if (product) {
          event.preventDefault();
          this.onSelect(product);
        }
        return;
      }
      case 'Escape': {
        if (this.isOpen()) {
          event.preventDefault();
          this.isOpen.set(false);
        }
        return;
      }
      default:
        return;
    }
  }

  protected onSelect(product: ProductLookup): void {
    this.selected.set(product);
    this.inputText.set(product.name);
    this.isOpen.set(false);
    this.activeIndex.set(-1);
    this.cva.emitChange(String(product.id));
  }

  protected onBlur(): void {
    this.isOpen.set(false);
    this.cva.markAsTouched();
  }

  private fallbackLabel(id: number): string {
    return `Product #${id}`;
  }
}
