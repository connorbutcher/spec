import { Component, computed, effect, inject, linkedSignal, untracked } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { PanelModule } from 'primeng/panel';
import { TabsModule } from 'primeng/tabs';
import { TagModule } from 'primeng/tag';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { DesignerToolbar } from '../designer-toolbar/designer-toolbar';
import { DesignerTab } from '../models/designer-tab';
import { PanelRef } from '../models/panel-ref';
import { TableTemplate } from '../models/table-template.model';
import { TemplateLayout } from '../models/template-layout.model';
import { TemplateSection } from '../models/template-section.model';
import { PanelNavigator } from '../panel-navigator';
import { sectionIcon } from '../section-links.util';
import { describeInstances, instanceTag, isHeader } from '../section-role.util';
import { StructureTree } from '../structure-tree/structure-tree';
import { TemplateCanvas } from '../template-canvas/template-canvas';
import { layoutTemplate, topLevelOrder } from '../template-layout.util';
import { TemplatesStore } from '../templates.store';
import { VersionBanner } from '../version-banner/version-banner';

/**
 * The middle column (`/templates/:templateId`): the open table's toolbar, a tab for each top-level
 * section (plus an overview of the whole table), the structure tree and a live preview. Each tab
 * designs one section on its own; opening it shows that section's settings on the right.
 */
@Component({
  selector: 'app-template-designer',
  imports: [
    ButtonModule,
    DesignerToolbar,
    EmptyState,
    MessageModule,
    PanelModule,
    StructureTree,
    TabsModule,
    TagModule,
    TemplateCanvas,
    VersionBanner,
  ],
  templateUrl: './template-designer.html',
  styleUrl: './template-designer.scss',
})
export class TemplateDesigner {
  /** The tab picked, back to the overview whenever another table is opened. */
  public readonly tab = linkedSignal<number | null, DesignerTab>({
    source: () => this.store.selectedTemplateId(),
    computation: () => 'overview',
  });

  /** The header, then the addable sections: one design tab each. */
  public readonly topSections = computed<TemplateSection[]>(() =>
    topLevelOrder(this.store.template()?.sections ?? []),
  );

  /** The tab showing: the picked one, or the overview if its section has gone. */
  public readonly activeTab = computed<DesignerTab>(() => {
    const tab = this.tab();
    return tab === 'overview' || this.topSections().some((section) => section.id === tab)
      ? tab
      : 'overview';
  });

  public readonly activeSection = computed<TemplateSection | null>(() => {
    const tab = this.activeTab();
    return tab === 'overview'
      ? null
      : (this.topSections().find((section) => section.id === tab) ?? null);
  });

  /** The preview for the tab: the whole table, or just the open section. */
  public readonly layout = computed<TemplateLayout | null>(() => {
    const template = this.store.template();
    const section = this.activeSection();
    if (!template) {
      return null;
    }
    return section ? layoutTemplate(template, section) : this.store.layout();
  });

  private readonly store = inject(TemplatesStore);
  private readonly navigator = inject(PanelNavigator);

  constructor() {
    // Opening something in the panel (from the tree, the preview or a breadcrumb) shows the tab of the
    // top-level section it belongs to. Only the panel's own navigation counts, so picking the overview
    // tab isn't undone by the next edit.
    effect(() => {
      const current = this.navigator.current();
      untracked(() => {
        const id = this.topLevelSectionOf(current);
        if (id !== null) {
          this.tab.set(id);
        }
      });
    });
  }

  public template(): TableTemplate | null {
    return this.store.template();
  }

  public isLoading(): boolean {
    return this.store.templateIsLoading() && this.store.template() === null;
  }

  public hasError(): boolean {
    return this.store.templateHasError();
  }

  public canEdit(): boolean {
    return this.store.canEdit();
  }

  public isSaving(): boolean {
    return this.store.isSaving();
  }

  public retry(): void {
    this.store.reloadTemplate();
  }

  public icon(section: TemplateSection): string {
    return sectionIcon(section);
  }

  public tag(section: TemplateSection): string {
    return instanceTag(section);
  }

  public isHeader(section: TemplateSection): boolean {
    return isHeader(section);
  }

  /** Switches tab, and opens that section's (or the table's) settings in the panel. */
  public selectTab(value: string | number | undefined): void {
    if (typeof value === 'number') {
      this.tab.set(value);
      this.navigator.open({ kind: 'section', id: value });
    } else {
      this.tab.set('overview');
      this.navigator.open({ kind: 'template' });
    }
  }

  public async addSection(): Promise<void> {
    const id = await this.store.addSection(null);
    if (id !== null) {
      this.tab.set(id);
      this.navigator.open({ kind: 'section', id });
    }
  }

  /** What the open tab is: a short explanation that these are added on the sheet by people, not fixed. */
  public intro(): string {
    const section = this.activeSection();
    const template = this.store.template();
    if (section === null) {
      const direction =
        template?.orientation === 'Vertical'
          ? 'Sections stack top to bottom.'
          : 'Sections run left to right.';
      return `The whole table. ${direction} The header is always there; every other section is added on the sheet by the people filling it in, not fixed here.`;
    }
    if (isHeader(section)) {
      return 'The header: always there, exactly one, and it comes first. It holds the heading rows only.';
    }
    return `This is one copy of "${section.name}". ${describeInstances(section)}. On the sheet, people add as many copies as they need, and each copy can have sub-sections added the same way.`;
  }

  private topLevelSectionOf(ref: PanelRef): number | null {
    const index = this.store.index();
    let sectionId: number | undefined;
    if (ref.kind === 'section') {
      sectionId = ref.id;
    } else if (ref.kind === 'row') {
      sectionId = index.rows.get(ref.id)?.section.id;
    } else if (ref.kind === 'cell') {
      sectionId = index.cells.get(ref.id)?.row.section.id;
    }
    const entry = sectionId === undefined ? undefined : index.sections.get(sectionId);
    return entry ? (entry.ancestors[0] ?? entry.section).id : null;
  }
}
