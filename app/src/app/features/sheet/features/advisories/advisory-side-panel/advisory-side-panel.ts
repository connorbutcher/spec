import { NgClass } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonDirective } from 'primeng/button';
import { ValidatorLevel } from '../../../../../core/types/configuration/validator-level.enum';
import { ISidePanelLayer } from '../../../../side-panel/models/side-panel-layer-data.interface';
import { SidePanelService } from '../../../../side-panel/side-panel.service';
import { SpecSheetAdvisory } from '../../../models/spec-sheet-advisory';
import { ValidatorLevelToAdvisoryIconPipe } from '../../../pipes/validator-level-to-advisory-icon';
import { ActiveSheetService } from '../../../services/active-sheet.service';

export type AdvisorySidePanelData = { level: ValidatorLevel | null };

@Component({
  selector: 'app-advisory-side-panel',
  imports: [ButtonDirective, NgClass, ValidatorLevelToAdvisoryIconPipe],
  templateUrl: './advisory-side-panel.html',
  styleUrl: './advisory-side-panel.scss',
})
export class AdvisorySidePanel
  implements ISidePanelLayer<AdvisorySidePanelData>
{
  public readonly level = signal<ValidatorLevel | null>(null);
  public readonly title = signal('Advisories');
  public readonly selectedAdvisory = signal<SpecSheetAdvisory | null>(null);
  public readonly levelLabels: Record<ValidatorLevel, string> = {
    [ValidatorLevel.Info]: 'Info',
    [ValidatorLevel.Warning]: 'Warning',
    [ValidatorLevel.Error]: 'Error',
  };

  public set data(data: AdvisorySidePanelData) {
    this.level.set(data.level);
  }

  public get data(): AdvisorySidePanelData {
    return { level: this.level() };
  }

  private readonly _advisoryGroups = computed(() => {
    const specSheet = this._activeSheetService.specSheet();
    return [
      {
        title: 'General',
        isTable: false,
        advisories: specSheet.specSheetAdvisories(),
      },
      ...specSheet.tables().map((table) => ({
        title: table.title() || 'Untitled Table',
        isTable: true,
        advisories: table.advisories(),
      })),
    ];
  });

  public readonly filterOptions = computed(() => {
    const advisories = this._advisoryGroups().flatMap(
      (group) => group.advisories,
    );
    return [
      { level: null, label: 'All', count: advisories.length },
      ...[
        ValidatorLevel.Info,
        ValidatorLevel.Warning,
        ValidatorLevel.Error,
      ].map((level) => ({
        level,
        label: this.levelLabels[level],
        count: advisories.filter((advisory) => advisory.level === level).length,
      })),
    ];
  });

  public readonly advisoryGroups = computed(() =>
    this._advisoryGroups()
      .map((group) => ({
        ...group,
        advisories: group.advisories.filter(
          (advisory) =>
            this.level() === null || advisory.level === this.level(),
        ),
      }))
      .filter((group) => group.advisories.length > 0),
  );
  public readonly visibleCount = computed(() =>
    this.advisoryGroups().reduce(
      (count, group) => count + group.advisories.length,
      0,
    ),
  );

  private readonly _activeSheetService = inject(ActiveSheetService);
  private readonly _sidePanelService = inject(SidePanelService);

  private readonly _router = inject(Router);
  private readonly _activatedRoute = inject(ActivatedRoute);

  public close(): void {
    this._sidePanelService.pop();
  }

  public goToIssue(issue: SpecSheetAdvisory): void {
    this.selectedAdvisory.set(issue);
    this._router.navigate([], {
      relativeTo: this._activatedRoute,
      fragment: issue.referenceId,
      queryParamsHandling: 'merge',
    });
  }
}
