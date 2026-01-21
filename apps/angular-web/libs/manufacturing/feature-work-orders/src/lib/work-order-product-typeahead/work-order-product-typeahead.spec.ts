import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import type { SearchResult } from '@adventureworks-web/shared/data-access';
import type { ProductLookup } from '@adventureworks-web/manufacturing/data-access';
import { WorkOrderProductTypeaheadComponent } from './work-order-product-typeahead';

const BASE_URL = 'https://api.test.com';
const SEARCH_URL = `${BASE_URL}/v1/products/search?pageNumber=1&pageSize=10`;
const PRODUCT_URL = `${BASE_URL}/v1/products/747`;

const mockEnvironment = {
  production: false,
  api: { primary: { baseUrl: BASE_URL, name: 'Test API' } },
};

const frameBlack: ProductLookup = { id: 747, name: 'HL Road Frame - Black, 58' };
const frameRed: ProductLookup = { id: 748, name: 'HL Road Frame - Red, 58' };

function searchResult(results: ProductLookup[] | null): SearchResult<ProductLookup> {
  return {
    pageNumber: 1,
    pageSize: 10,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false,
    totalRecords: results?.length ?? 0,
    results,
  };
}

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, WorkOrderProductTypeaheadComponent],
  template: `<aw-work-order-product-typeahead fieldId="t" label="Product" [formControl]="control" />`,
})
class HostComponent {
  readonly control = new FormControl('', { nonNullable: true });
}

describe('WorkOrderProductTypeaheadComponent', () => {
  let fixture: ComponentFixture<HostComponent>;
  let host: HostComponent;
  let httpTesting: HttpTestingController;

  const inputEl = (): HTMLInputElement => fixture.nativeElement.querySelector('#t-input');
  const listboxEl = (): HTMLElement => fixture.nativeElement.querySelector('#t-listbox');
  const optionEls = (): HTMLElement[] => Array.from(fixture.nativeElement.querySelectorAll('[role="option"]'));
  const statusText = (): string => (fixture.nativeElement.querySelector('#t-status') as HTMLElement).textContent.trim();

  function typeText(text: string): void {
    inputEl().value = text;
    inputEl().dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  function pressKey(key: string): KeyboardEvent {
    const event = new KeyboardEvent('keydown', { key, cancelable: true, bubbles: true });
    inputEl().dispatchEvent(event);
    fixture.detectChanges();
    return event;
  }

  /** Types a term, lets the debounce elapse, and answers the search request. */
  function searchAndRespond(term: string, results: ProductLookup[]): void {
    typeText(term);
    vi.advanceTimersByTime(300);
    httpTesting.expectOne(SEARCH_URL).flush(searchResult(results));
    fixture.detectChanges();
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HostComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: ENVIRONMENT, useValue: mockEnvironment }],
    }).compileComponents();

    vi.useFakeTimers();
    httpTesting = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(HostComponent);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    httpTesting.verify();
    vi.useRealTimers();
  });

  describe('searching', () => {
    it('waits for the 300ms debounce before searching', () => {
      typeText('fra');

      vi.advanceTimersByTime(299);
      httpTesting.expectNone(SEARCH_URL);

      vi.advanceTimersByTime(1);
      const req = httpTesting.expectOne(SEARCH_URL);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ name: 'fra' });
      req.flush(searchResult([]));
    });

    it('searches only the latest term while the user keeps typing', () => {
      typeText('f');
      vi.advanceTimersByTime(100);
      typeText('fr');
      vi.advanceTimersByTime(100);
      typeText('fra');
      vi.advanceTimersByTime(300);

      const req = httpTesting.expectOne(SEARCH_URL);
      expect(req.request.body).toEqual({ name: 'fra' });
      req.flush(searchResult([]));
    });

    it('trims the term and never searches for blank text', () => {
      typeText('   ');
      vi.advanceTimersByTime(300);

      httpTesting.expectNone(SEARCH_URL);
    });

    it('caps the input at the API name length of 50', () => {
      expect(inputEl().getAttribute('maxlength')).toBe('50');
    });

    it('lists the matching products', () => {
      searchAndRespond('frame', [frameBlack, frameRed]);

      expect(optionEls().map((option) => option.textContent.trim())).toEqual([frameBlack.name, frameRed.name]);
      expect(listboxEl().hidden).toBe(false);
      expect(inputEl().getAttribute('aria-expanded')).toBe('true');
    });

    it('says so when nothing matches', () => {
      searchAndRespond('zzz', []);

      expect(statusText()).toBe('No products found.');
      expect(listboxEl().hidden).toBe(true);
    });

    it('treats a null result list as no matches', () => {
      typeText('zzz');
      vi.advanceTimersByTime(300);
      httpTesting.expectOne(SEARCH_URL).flush(searchResult(null));
      fixture.detectChanges();

      expect(statusText()).toBe('No products found.');
    });

    it('reports an unavailable search without throwing, and keeps working afterwards', () => {
      typeText('frame');
      vi.advanceTimersByTime(300);
      httpTesting.expectOne(SEARCH_URL).flush('boom', { status: 500, statusText: 'Server Error' });
      fixture.detectChanges();

      expect(statusText()).toBe('Product search is unavailable.');

      searchAndRespond('frame', [frameBlack]);
      expect(optionEls()).toHaveLength(1);
      expect(statusText()).toBe('');
    });
  });

  describe('selecting', () => {
    it('sets the product id on the form control and shows the name when an option is clicked', () => {
      searchAndRespond('frame', [frameBlack, frameRed]);

      optionEls()[0].click();
      fixture.detectChanges();

      expect(host.control.value).toBe('747');
      expect(inputEl().value).toBe(frameBlack.name);
      expect(listboxEl().hidden).toBe(true);
    });

    it('selects with ArrowDown and Enter', () => {
      searchAndRespond('frame', [frameBlack, frameRed]);

      pressKey('ArrowDown');
      pressKey('ArrowDown');

      expect(inputEl().getAttribute('aria-activedescendant')).toBe('t-option-1');
      expect(optionEls()[1].getAttribute('aria-selected')).toBe('true');

      const enter = pressKey('Enter');

      expect(enter.defaultPrevented).toBe(true);
      expect(host.control.value).toBe('748');
    });

    it('wraps ArrowUp from the first option to the last', () => {
      searchAndRespond('frame', [frameBlack, frameRed]);

      pressKey('ArrowDown');
      pressKey('ArrowUp');

      expect(inputEl().getAttribute('aria-activedescendant')).toBe('t-option-1');
    });

    it('lets Enter through to the form when no option is active', () => {
      searchAndRespond('frame', [frameBlack]);

      const enter = pressKey('Enter');

      expect(enter.defaultPrevented).toBe(false);
      expect(host.control.value).toBe('');
    });

    it('closes the list on Escape without selecting', () => {
      searchAndRespond('frame', [frameBlack]);

      const escape = pressKey('Escape');

      expect(escape.defaultPrevented).toBe(true);
      expect(listboxEl().hidden).toBe(true);
      expect(host.control.value).toBe('');
    });

    it('clears the selected id as soon as the user edits the text', () => {
      searchAndRespond('frame', [frameBlack]);
      optionEls()[0].click();
      fixture.detectChanges();
      expect(host.control.value).toBe('747');

      typeText('HL Road Frame - Black, 5');

      expect(host.control.value).toBe('');
    });

    it('marks the control touched on blur', () => {
      expect(host.control.touched).toBe(false);

      inputEl().dispatchEvent(new Event('blur'));

      expect(host.control.touched).toBe(true);
    });
  });

  describe('restoring a value from the form', () => {
    it('labels a product id from the URL by fetching the product', () => {
      host.control.setValue('747');
      fixture.detectChanges();

      expect(inputEl().value).toBe('Product #747');

      httpTesting.expectOne(PRODUCT_URL).flush(frameBlack);
      fixture.detectChanges();

      expect(inputEl().value).toBe(frameBlack.name);
    });

    it('keeps the id label and the filter value when the product lookup fails', () => {
      host.control.setValue('747');
      fixture.detectChanges();

      httpTesting.expectOne(PRODUCT_URL).flush('Not Found', { status: 404, statusText: 'Not Found' });
      fixture.detectChanges();

      expect(inputEl().value).toBe('Product #747');
      expect(host.control.value).toBe('747');
    });

    it.each(['abc', '-5', '0', '1.5', '12abc'])('never sends the junk value "%s" to the API', (junk) => {
      host.control.setValue(junk);
      fixture.detectChanges();

      httpTesting.expectNone((req) => req.url.includes('/v1/products/'));
      expect(inputEl().value).toBe('');
    });

    it('does not refetch the name for the id the user just picked', () => {
      searchAndRespond('frame', [frameBlack]);
      optionEls()[0].click();
      fixture.detectChanges();

      // The list re-writes the same id back after the URL round-trip.
      host.control.setValue('747');
      fixture.detectChanges();

      httpTesting.expectNone(PRODUCT_URL);
      expect(inputEl().value).toBe(frameBlack.name);
    });

    it('clears the text when the form resets the value', () => {
      searchAndRespond('frame', [frameBlack]);
      optionEls()[0].click();
      fixture.detectChanges();

      host.control.setValue('');
      fixture.detectChanges();

      expect(inputEl().value).toBe('');
    });

    it('ignores a product name that arrives after the value was cleared', () => {
      host.control.setValue('747');
      fixture.detectChanges();
      host.control.setValue('');
      fixture.detectChanges();

      httpTesting.expectOne(PRODUCT_URL).flush(frameBlack);
      fixture.detectChanges();

      expect(inputEl().value).toBe('');
    });
  });

  describe('accessibility wiring', () => {
    it('points aria-controls at a listbox that exists even while closed', () => {
      const controls = inputEl().getAttribute('aria-controls');

      expect(controls).toBe('t-listbox');
      expect(listboxEl()).not.toBeNull();
      expect(listboxEl().hidden).toBe(true);
      expect(inputEl().getAttribute('role')).toBe('combobox');
      expect(inputEl().getAttribute('aria-expanded')).toBe('false');
    });

    it('labels the input with the visible label text', () => {
      const label = fixture.nativeElement.querySelector('label[for="t-input"]') as HTMLElement;

      expect(label.textContent.trim()).toBe('Product');
    });

    it('disables the input when the control is disabled', () => {
      host.control.disable();
      fixture.detectChanges();

      expect(inputEl().disabled).toBe(true);
    });
  });
});
