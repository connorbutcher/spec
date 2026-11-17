import {
  afterNextRender,
  Component,
  computed,
  ElementRef,
  inject,
  linkedSignal,
  signal,
} from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { ISidePanelLayer } from '../../../../side-panel/models/side-panel-layer-data.interface';
import { SidePanelService } from '../../../../side-panel/side-panel.service';
import { ActiveSheetService } from '../../../services/active-sheet.service';
import { ActiveSystemReference } from '../active-system-reference/active-system-reference';
import { SystemReference } from '../models/system-reference';
import { SystemReferenceGroup } from '../models/system-reference-group';

export type ReferencesSidePanelData = { referenceId?: string };

@Component({
  selector: 'app-references-side-panel',
  imports: [ButtonDirective, ActiveSystemReference],
  templateUrl: './references-side-panel.html',
  styleUrl: './references-side-panel.scss',
})
export class ReferencesSidePanel
  implements ISidePanelLayer<ReferencesSidePanelData>
{
  private _data: ReferencesSidePanelData = {};

  public set data(data: ReferencesSidePanelData) {
    this._data = data;
    this.highlightedReferenceId.set(data.referenceId);
    const reference = this._activeSheetService
      .externalReferences()
      .find((reference) => reference.referenceId === data.referenceId);
    if (reference) {
      this.selectedSystem.set(reference.systemName);
    }
  }

  public get data(): ReferencesSidePanelData {
    return this._data;
  }

  public readonly title = signal('External References');
  public readonly highlightedReferenceId = signal<string | undefined>(
    undefined,
  );

  private readonly _activeSheetService = inject(ActiveSheetService);
  private readonly _sidePanelService = inject(SidePanelService);
  private readonly _elementRef = inject<ElementRef<HTMLElement>>(ElementRef);

  constructor() {
    afterNextRender(() => {
      const referenceId = this.data.referenceId;
      if (!referenceId) {
        return;
      }

      const rows = this._elementRef.nativeElement.querySelectorAll<HTMLElement>(
        '[data-reference-id]',
      );
      Array.from(rows)
        .find((row) => row.dataset['referenceId'] === referenceId)
        ?.scrollIntoView({ block: 'center', inline: 'nearest' });
    });
  }

  public readonly systemReferences = computed<SystemReference[]>(() => {
    const systems = new Map<string, SystemReference>();
    const groups = new Map<string, Map<string, SystemReferenceGroup>>();

    for (const reference of this._activeSheetService.externalReferences()) {
      let system = systems.get(reference.systemName);
      if (!system) {
        system = {
          system: reference.systemName,
          iconUrl: reference.systemIconUrl,
          referenceGroups: [],
        };
        systems.set(reference.systemName, system);
        groups.set(reference.systemName, new Map());
      }

      const systemGroups = groups.get(reference.systemName)!;
      const groupName = reference.grouping ?? 'Ungrouped';
      let group = systemGroups.get(groupName);
      if (!group) {
        group = { group: groupName, references: [] };
        systemGroups.set(groupName, group);
        system.referenceGroups.push(group);
      }
      group.references.push(reference);
    }

    return Array.from(systems.values());
  });

  public readonly systemOptions = computed(() =>
    this.systemReferences().map((system) => ({
      ...system,
      referenceCount: system.referenceGroups.reduce(
        (count, group) => count + group.references.length,
        0,
      ),
    })),
  );

  public readonly selectedSystem = linkedSignal<
    SystemReference[],
    string | undefined
  >({
    source: this.systemReferences,
    computation: (systems, previous) =>
      systems.find((system) => system.system === previous?.value)?.system ??
      systems[0]?.system,
  });
  public readonly activeSystem = computed(() =>
    this.systemReferences().find(
      (system) => system.system === this.selectedSystem(),
    ),
  );

  public close(): void {
    this._sidePanelService.pop();
  }
}
