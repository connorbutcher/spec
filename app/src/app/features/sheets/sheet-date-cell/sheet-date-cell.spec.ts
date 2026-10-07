import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CellValueRequest } from '../models/cell-value-request.model';
import { fixtureCell } from '../sheet-structure.fixture';
import { SheetDateCell } from './sheet-date-cell';

describe('SheetDateCell', () => {
  let fixture: ComponentFixture<SheetDateCell>;
  let saved: CellValueRequest[];

  function input(): HTMLInputElement {
    return (fixture.nativeElement as HTMLElement).querySelector('input') as HTMLInputElement;
  }

  /** Types text the way a keyboard does: the date picker only reads input that follows a key press. */
  function type(text: string): void {
    const box = input();
    box.dispatchEvent(new KeyboardEvent('keydown', { key: text.slice(-1), bubbles: true }));
    box.value = text;
    box.dispatchEvent(new Event('input', { bubbles: true }));
  }

  async function render(dateValue: string | null): Promise<void> {
    fixture = TestBed.createComponent(SheetDateCell);
    fixture.componentRef.setInput('cell', { ...fixtureCell(1), dateValue });
    saved = [];
    fixture.componentInstance.changed.subscribe((request) => saved.push(request));
    await fixture.whenStable();
  }

  it('shows the saved date as yyyy-mm-dd', async () => {
    await render('2026-09-12');

    expect(input().value).toBe('2026-09-12');
  });

  it('saves a date that was typed, once focus leaves the box', async () => {
    await render(null);

    type('2026-10-01');
    expect(saved).toEqual([]);

    input().dispatchEvent(new FocusEvent('blur'));

    expect(saved).toHaveLength(1);
    expect(saved[0].date).toBe('2026-10-01');
  });

  it('saves nothing when the box is left as it was', async () => {
    await render('2026-09-12');

    input().dispatchEvent(new FocusEvent('blur'));

    expect(saved).toEqual([]);
  });

  it('clears the cell when the date is deleted', async () => {
    await render('2026-09-12');

    type('');
    input().dispatchEvent(new FocusEvent('blur'));

    expect(saved).toHaveLength(1);
    expect(saved[0].date).toBeNull();
  });

  it('does not save half a date, and puts the saved one back', async () => {
    await render('2026-09-12');

    type('2026-1');
    input().dispatchEvent(new FocusEvent('blur'));
    await fixture.whenStable();

    expect(saved).toEqual([]);
    expect(input().value).toBe('2026-09-12');
  });
});
