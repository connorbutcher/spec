import { Component, effect, inject, untracked } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ConfigPanel } from '../config-panel/config-panel';
import { PanelNavigator } from '../panel-navigator';
import { TemplateListPanel } from '../template-list-panel/template-list-panel';
import { TemplatesStore } from '../templates.store';

/**
 * The templates screen: sheet types and their tables on the left, the open table's designer in the
 * middle and the configuration panel on the right.
 */
@Component({
  selector: 'app-templates-page',
  imports: [RouterOutlet, TemplateListPanel, ConfigPanel],
  providers: [TemplatesStore, PanelNavigator],
  templateUrl: './templates-page.html',
  styleUrl: './templates-page.scss',
})
export class TemplatesPage {
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
