import { SheetStructureFixture } from '../data/sheet-structure.fixture';
import {
  SheetLabelsUtil } from './sheet-labels.util';

describe('sheet labels', () => {
  it('names a row after the first text typed into it, if there is any', () => {
    const named = SheetStructureFixture.fixtureRow([SheetStructureFixture.fixtureCell(1), { ...SheetStructureFixture.fixtureCell(2), textValue: 'Stem diameter' }]);

    expect(SheetLabelsUtil.rowLabel(named)).toBe('Stem diameter');
    expect(SheetLabelsUtil.rowLabel(SheetStructureFixture.fixtureRow([SheetStructureFixture.fixtureCell(1)]))).toBeNull();
  });

  it('names a section after its template, plus the first text typed into it', () => {
    const empty = SheetStructureFixture.fixtureSection('Group', 'Addable', [SheetStructureFixture.fixtureRow([SheetStructureFixture.fixtureCell(1)])]);
    const named = SheetStructureFixture.fixtureSection('Group', 'Addable', [
      SheetStructureFixture.fixtureRow([SheetStructureFixture.fixtureCell(1), { ...SheetStructureFixture.fixtureCell(2), textValue: 'Intake valve' }]),
    ]);

    expect(SheetLabelsUtil.sectionLabel(empty)).toBe('Group');
    expect(SheetLabelsUtil.sectionLabel(named)).toBe('Group · Intake valve');
  });

  it('calls a table by its title, or by its template until it has one', () => {
    const table = SheetStructureFixture.fixtureTable();

    expect(SheetLabelsUtil.tableLabel(table)).toBe('Limits table');
    expect(SheetLabelsUtil.tableLabel({ ...table, title: 'Piston parts' })).toBe('Piston parts');
  });

  it('writes a version as v and its number', () => {
    expect(SheetLabelsUtil.versionLabel(3)).toBe('v3');
  });

  it('says what is being looked at, and what changes are marked since', () => {
    const live = {
      isLive: true,
      latestVersionNumber: 9,
      viewedVersionNumber: null,
      viewedAsOfUtc: null,
    };

    expect(SheetLabelsUtil.viewLabel(live, null)).toBe('Live · v9');
    expect(SheetLabelsUtil.viewLabel(live, 6)).toBe('Live · v9 · changes since v6');
    expect(SheetLabelsUtil.viewLabel({ ...live, latestVersionNumber: null }, null)).toBe('Live');
    expect(SheetLabelsUtil.viewLabel({ ...live, isLive: false, viewedVersionNumber: 6 }, null)).toBe(
      'v6 · read-only',
    );
  });

  it('says which version changed something, when and by whom', () => {
    const label = SheetLabelsUtil.changeLabel({
      versionNumber: 3,
      atUtc: '2026-09-12T10:00:00Z',
      userName: 'A. Smith',
    });

    expect(label.startsWith('v3 · ')).toBe(true);
    expect(label.endsWith(' · A. Smith')).toBe(true);
    expect(label).toContain('2026');
  });
});
