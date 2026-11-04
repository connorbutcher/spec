import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CellValueRequest } from '../models/cell-value-request.model';
import { SheetStructureFixture } from '../sheet-structure.fixture';
import { SheetDateCell } from './sheet-date-cell';

class SheetDateCellPage {
  public fixture!: ComponentFixture<SheetDateCell>;
  public saved: CellValueRequest[] = [];

  public input(): HTMLInputElement {
    return (this.fixture.nativeElement as HTMLElement).querySelector('input') as HTMLInputElement;
  }

  /** Types text the way a keyboard does: the date picker only reads input that follows a key press. */
  public type(text: string): void {
    const box = this.input();
    box.dispatchEvent(new KeyboardEvent('keydown', { key: text.slice(-1), bubbles: true }));
    box.value = text;
    box.dispatchEvent(new Event('input', { bubbles: true }));
  }

  public async render(dateValue: string | null): Promise<void> {
    this.fixture = TestBed.createComponent(SheetDateCell);
    this.fixture.componentRef.setInput('cell', { ...SheetStructureFixture.fixtureCell(1), dateValue });
    this.saved = [];
    this.fixture.componentInstance.changed.subscribe((request) => this.saved.push(request));
    await this.fixture.whenStable();
  }
}

describe('SheetDateCell', () => {
  let page: SheetDateCellPage;

  beforeEach(() => {
    page = new SheetDateCellPage();
  });

  it('shows the saved date as yyyy-mm-dd', async () => {
    await page.render('2026-09-12');

    expect(page.input().value).toBe('2026-09-12');
  });

  it('saves a date that was typed, once focus leaves the box', async () => {
    await page.render(null);

    page.type('2026-10-01');
    expect(page.saved).toEqual([]);

    page.input().dispatchEvent(new FocusEvent('blur'));

    expect(page.saved).toHaveLength(1);
    expect(page.saved[0].date).toBe('2026-10-01');
  });

  it('saves nothing when the box is left as it was', async () => {
    await page.render('2026-09-12');

    page.input().dispatchEvent(new FocusEvent('blur'));

    expect(page.saved).toEqual([]);
  });

  it('clears the cell when the date is deleted', async () => {
    await page.render('2026-09-12');

    page.type('');
    page.input().dispatchEvent(new FocusEvent('blur'));

    expect(page.saved).toHaveLength(1);
    expect(page.saved[0].date).toBeNull();
  });

  it('does not save half a date, and puts the saved one back', async () => {
    await page.render('2026-09-12');

    page.type('2026-1');
    page.input().dispatchEvent(new FocusEvent('blur'));
    await page.fixture.whenStable();

    expect(page.saved).toEqual([]);
    expect(page.input().value).toBe('2026-09-12');
  });
});
