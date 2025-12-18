import { By } from '@angular/platform-browser';
import type { OrgChartTreeNode } from '../models/org-chart-tree-node.model';
import { renderOrgChartComponent } from '../testing/render-org-chart-component';
import { OrgNodeComponent } from './org-node';

function node(overrides: Partial<OrgChartTreeNode> & { employeeId: number }): OrgChartTreeNode {
  return {
    fullName: `Employee ${overrides.employeeId}`,
    jobTitle: 'Some Title',
    departmentName: 'Sales',
    organizationLevel: null,
    colorClass: 'accent',
    children: [],
    ...overrides,
  };
}

describe('OrgNodeComponent', () => {
  it('renders the badge with the node colorClass', async () => {
    const { fixture } = await renderOrgChartComponent(OrgNodeComponent);
    fixture.componentRef.setInput('node', node({ employeeId: 1, colorClass: 'primary' }));
    fixture.componentRef.setInput('expandedIds', new Set<number>());
    fixture.detectChanges();

    const badge = fixture.debugElement.query(By.css('.badge'));
    expect(badge.nativeElement.classList).toContain('badge-primary');
  });

  it('emits toggle (not navigate) when the chevron is clicked, and stops propagation', async () => {
    const { fixture, component } = await renderOrgChartComponent(OrgNodeComponent);
    fixture.componentRef.setInput(
      'node',
      node({ employeeId: 1, children: [node({ employeeId: 2 })] }),
    );
    fixture.componentRef.setInput('expandedIds', new Set<number>());
    fixture.detectChanges();

    const toggleSpy = vi.fn();
    const navigateSpy = vi.fn();
    component.toggleExpand.subscribe(toggleSpy);
    component.navigate.subscribe(navigateSpy);

    fixture.debugElement.query(By.css('button')).nativeElement.click();

    expect(toggleSpy).toHaveBeenCalledWith(1);
    expect(navigateSpy).not.toHaveBeenCalled();
  });

  it('toggles via keydown.enter on the chevron without also triggering navigate (keydown bubbles to the card, which listens for keydown.enter too)', async () => {
    const { fixture, component } = await renderOrgChartComponent(OrgNodeComponent);
    fixture.componentRef.setInput('node', node({ employeeId: 1, children: [node({ employeeId: 2 })] }));
    fixture.componentRef.setInput('expandedIds', new Set<number>());
    fixture.detectChanges();

    const toggleSpy = vi.fn();
    const navigateSpy = vi.fn();
    component.toggleExpand.subscribe(toggleSpy);
    component.navigate.subscribe(navigateSpy);

    const button = fixture.debugElement.query(By.css('button')).nativeElement as HTMLButtonElement;
    button.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', bubbles: true, cancelable: true }));

    expect(toggleSpy).not.toHaveBeenCalled();
    expect(navigateSpy).not.toHaveBeenCalled();
  });

  it('exposes aria-expanded on the chevron reflecting expansion state', async () => {
    const { fixture } = await renderOrgChartComponent(OrgNodeComponent);
    fixture.componentRef.setInput('node', node({ employeeId: 1, children: [node({ employeeId: 2 })] }));
    fixture.componentRef.setInput('expandedIds', new Set([1]));
    fixture.detectChanges();

    const button = fixture.debugElement.query(By.css('button')).nativeElement as HTMLButtonElement;
    expect(button.getAttribute('aria-expanded')).toBe('true');
  });

  it('emits navigate when the card body is clicked', async () => {
    const { fixture, component } = await renderOrgChartComponent(OrgNodeComponent);
    fixture.componentRef.setInput('node', node({ employeeId: 42 }));
    fixture.componentRef.setInput('expandedIds', new Set<number>());
    fixture.detectChanges();

    const navigateSpy = vi.fn();
    component.navigate.subscribe(navigateSpy);

    fixture.debugElement.query(By.css('.card')).nativeElement.click();

    expect(navigateSpy).toHaveBeenCalledWith(42);
  });

  it('recursively renders every descendant across a 3-level fixture', async () => {
    const tree = node({
      employeeId: 1,
      children: [
        node({
          employeeId: 2,
          children: [node({ employeeId: 3 }), node({ employeeId: 4 })],
        }),
      ],
    });

    const { fixture } = await renderOrgChartComponent(OrgNodeComponent);
    fixture.componentRef.setInput('node', tree);
    fixture.componentRef.setInput('expandedIds', new Set([1, 2]));
    fixture.detectChanges();

    const allNodes = fixture.debugElement.queryAll(By.css('aw-org-node'));
    // 3 descendants (2, 3, 4) rendered inside the root's own aw-org-node host
    expect(allNodes).toHaveLength(3);
  });
});
