import { provideTranslateService } from '@ngx-translate/core';
import type { EmployeePayHistory } from '@adventureworks-web/hr/data-access';
import { renderHistoryComponent } from '../testing/render-history-component';
import { PayHistoryTableComponent } from './pay-history-table';

// Employee 250 (Sheela Word) — verified against the local AdventureWorks DB
const payHistory: EmployeePayHistory[] = [
  { rateChangeDate: '2011-02-21', rate: 9.86, payFrequency: 2, payFrequencyLabel: 'Bi-Weekly' },
  { rateChangeDate: '2011-07-30', rate: 22.5, payFrequency: 2, payFrequencyLabel: 'Bi-Weekly' },
  { rateChangeDate: '2012-07-14', rate: 30.0, payFrequency: 2, payFrequencyLabel: 'Bi-Weekly' },
];

describe('PayHistoryTableComponent', () => {
  async function setup(history: EmployeePayHistory[] = payHistory) {
    const { fixture, component } = await renderHistoryComponent(PayHistoryTableComponent, [provideTranslateService()]);
    fixture.componentRef.setInput('payHistory', history);
    fixture.detectChanges();
    return { fixture, component };
  }

  it('renders rows most-recent-first', async () => {
    const { fixture } = await setup();

    const rateCells = Array.from<Element>(
      fixture.nativeElement.querySelectorAll('#aw-pay-history-table-table tbody tr td:first-child'),
    ).map((el) => el.textContent?.trim());
    expect(rateCells[0]).toContain('Jul 14, 2012');
    expect(rateCells[2]).toContain('Feb 21, 2011');
  });

  it('shows an Initial badge for the oldest entry', async () => {
    const { fixture } = await setup();

    const rows = fixture.nativeElement.querySelectorAll('#aw-pay-history-table-table tbody tr');
    const oldestRow = rows[rows.length - 1];
    expect(oldestRow.querySelector('aw-status-badge')).toBeTruthy();
  });

  it('shows a green up-arrow increase for a higher rate', async () => {
    const { fixture } = await setup();

    const rows = fixture.nativeElement.querySelectorAll('#aw-pay-history-table-table tbody tr');
    expect(rows[0].querySelector('.text-success .fa-arrow-up')).toBeTruthy();
  });

  it('shows a red down-arrow decrease for a lower rate', async () => {
    const decreasing: EmployeePayHistory[] = [
      { rateChangeDate: '2020-01-01', rate: 30, payFrequency: 2, payFrequencyLabel: 'Bi-Weekly' },
      { rateChangeDate: '2020-06-01', rate: 25, payFrequency: 2, payFrequencyLabel: 'Bi-Weekly' },
    ];
    const { fixture } = await setup(decreasing);

    const rows = fixture.nativeElement.querySelectorAll('#aw-pay-history-table-table tbody tr');
    expect(rows[0].querySelector('.text-error .fa-arrow-down')).toBeTruthy();
  });

  it('renders the empty state for an empty history', async () => {
    const { fixture } = await setup([]);

    expect(fixture.nativeElement.querySelector('#aw-pay-history-table-empty')).toBeTruthy();
  });
});
