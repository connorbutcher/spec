import { Component, computed, effect, inject, untracked } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ConfirmationService } from 'primeng/api';
import { ConfirmPopupModule } from 'primeng/confirmpopup';
import { ConfigPanel } from '../config-panel/config-panel';
import { PanelNavigator } from '../panel-navigator';
import { SettingsGroupState } from '../settings-group-state';
import { StructureActions } from '../structure-actions';
import { TemplateListPanel } from '../template-list-panel/template-list-panel';
import { TemplatesRail } from '../templates-rail/templates-rail';
import { CellTypesStore } from '../cell-types.store';
import { TemplateSaves } from '../template-saves';
import { TemplatesStore } from '../templates.store';

/**
 * The templates screen: sheet types and their tables on the left, the open table's designer in the
 * middle and the configuration panel on the right. Once a table is open the list folds into a rail,
 * so the designer has the room.
 */
@Component({
  selector: 'app-templates-page',
  imports: [RouterOutlet, ConfirmPopupModule, TemplateListPanel, TemplatesRail, ConfigPanel],
  providers: [
    TemplatesStore,
    CellTypesStore,
    TemplateSaves,
    PanelNavigator,
    ConfirmationService,
    StructureActions,
    SettingsGroupState,
  ],
  templateUrl: './templates-page.html',
  styleUrl: './templates-page.scss',
})
export class TemplatesPage {
  public readonly isDesigning = computed(() => this.store.selectedTemplateId() !== null);

  private readonly store = inject(TemplatesStore);
  private readonly navigator = inject(PanelNavigator);

  constructor() {
    // Opening a different table starts the panel's history again at that table's settings.
    effect(() => {
      const templateId = this.store.selectedTemplateId();
      untracked(() =>
        this.navigator.reset(templateId === null ? { kind: 'cellTypes' } : { kind: 'template' }),
      );
    });
  }
}
