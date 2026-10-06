import { fixtureCell, fixtureRow, fixtureSection, fixtureTable } from './sheet-structure.fixture';
import { changeLabel, sectionLabel, tableLabel, versionLabel } from './sheet-labels.util';

describe('sheet labels', () => {
  it('names a section after its template, plus the first text typed into it', () => {
    const empty = fixtureSection('Group', 'Addable', [fixtureRow([fixtureCell(1)])]);
    const named = fixtureSection('Group', 'Addable', [
      fixtureRow([fixtureCell(1), { ...fixtureCell(2), textValue: 'Intake valve' }]),
    ]);

    expect(sectionLabel(empty)).toBe('Group');
    expect(sectionLabel(named)).toBe('Group · Intake valve');
  });

  it('calls a table by its title, or by its template until it has one', () => {
    const table = fixtureTable();

    expect(tableLabel(table)).toBe('Limits table');
    expect(tableLabel({ ...table, title: 'Piston parts' })).toBe('Piston parts');
  });

  it('writes a version as v and its number', () => {
    expect(versionLabel(3)).toBe('v3');
  });

  it('says which version changed something, when and by whom', () => {
    const label = changeLabel({
      versionNumber: 3,
      atUtc: '2026-09-12T10:00:00Z',
      userName: 'A. Smith',
    });

    expect(label.startsWith('v3 · ')).toBe(true);
    expect(label.endsWith(' · A. Smith')).toBe(true);
    expect(label).toContain('2026');
  });
});
