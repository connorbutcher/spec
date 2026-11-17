import { NgClass } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { ButtonGroup } from 'primeng/buttongroup';
import { Tooltip } from 'primeng/tooltip';
import { ValidatorLevel } from '../../../../../core/types/configuration/validator-level.enum';
import { SidePanelService } from '../../../../side-panel/side-panel.service';
import { SpecSheetAdvisory } from '../../../models/spec-sheet-advisory';
import { ValidatorLevelToAdvisoryIconPipe } from '../../../pipes/validator-level-to-advisory-icon';
import { ValidatorLevelToAdvisorySeverityPipe } from '../../../pipes/validator-level-to-advisory-severity';
import { ValidatorLevelToAdvisoryTooltipPipe } from '../../../pipes/validator-level-to-advisory-tooltip';
import { ActiveSheetService } from '../../../services/active-sheet.service';
import {
  AdvisorySidePanel,
  AdvisorySidePanelData,
} from '../advisory-side-panel/advisory-side-panel';

type AdvisoryGroup = {
  level: ValidatorLevel;
  advisories: SpecSheetAdvisory[];
};

@Component({
  selector: 'app-advisory-overview',
  imports: [
    NgClass,
    ButtonDirective,
    ButtonGroup,
    Tooltip,
    ValidatorLevelToAdvisoryIconPipe,
    ValidatorLevelToAdvisorySeverityPipe,
    ValidatorLevelToAdvisoryTooltipPipe,
  ],
  templateUrl: './advisory-overview.html',
  styleUrl: './advisory-overview.scss',
})
export class AdvisoryOverview {
  public readonly advisoryGroups = computed<AdvisoryGroup[]>(() => {
    const specSheet = this._activeSheetService.specSheet();
    const specSheetAdvisories = specSheet.specSheetAdvisories();
    const tableAdvisories = specSheet.advisories();

    const allAdvisories = [...specSheetAdvisories, ...tableAdvisories];
    const groups: Map<ValidatorLevel, SpecSheetAdvisory[]> = new Map([
      [ValidatorLevel.Info, []],
      [ValidatorLevel.Warning, []],
      [ValidatorLevel.Error, []],
    ]);

    for (const advisory of allAdvisories) {
      groups.get(advisory.level)?.push(advisory);
    }

    return Array.from(groups.entries()).map(([level, advisories]) => ({
      level,
      advisories,
    }));
  });

  private readonly _activeSheetService = inject(ActiveSheetService);
  private readonly _sidePanelService = inject(SidePanelService);

  public viewAdvisories(group: AdvisoryGroup): void {
    if (group.advisories.length === 0) {
      return;
    }

    const panelId = 'spec-sheet-advisories';
    const activePanel = this._sidePanelService.activePanel();
    if (activePanel && activePanel?.component === AdvisorySidePanel) {
      activePanel.data = { level: group.level };
      this._sidePanelService.refresh();
      this._sidePanelService.setShowPanel(true);
      return;
    }

    this._sidePanelService.push<AdvisorySidePanel, AdvisorySidePanelData>(
      AdvisorySidePanel,
      { level: group.level },
      panelId,
    );
  }
}
