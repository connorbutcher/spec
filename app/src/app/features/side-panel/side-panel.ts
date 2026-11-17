import { A11yModule } from '@angular/cdk/a11y';
import {
  AfterViewInit,
  Component,
  computed,
  ElementRef,
  inject,
  OnInit,
  Signal,
  viewChild,
  ViewContainerRef,
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { Tooltip } from 'primeng/tooltip';
import { combineLatest } from 'rxjs';
import { delay, takeUntil } from 'rxjs/operators';
import { Destroyable } from '../common/destructable/destructable';
import { SidePanelConfigService } from './side-panel-config.service';
import { SidePanelService } from './side-panel.service';

@Component({
  selector: 'app-side-panel',
  imports: [Tooltip, A11yModule],
  templateUrl: './side-panel.html',
  styleUrl: './side-panel.scss',
})
export class SidePanel extends Destroyable implements OnInit, AfterViewInit {
  public readonly sidePanelContent = viewChild('sidePanelContent', {
    read: ViewContainerRef,
  });
  public readonly dotList = viewChild('dotList', { read: ElementRef });
  public readonly panelWidth = computed(
    () => this._sidePanelConfigService?.panelWidth() ?? 500,
  );
  public readonly canNavigate = computed(
    () => this._sidePanelConfigService?.canNavigate() ?? true,
  );
  public readonly canClose = computed(
    () => this._sidePanelConfigService?.canClose() ?? true,
  );

  private readonly _sidePanelConfigService = inject(SidePanelConfigService, {
    optional: true,
  });

  constructor(private readonly _sidePanelService: SidePanelService) {
    super();

    combineLatest([
      toObservable(this._sidePanelService.activePanel),
      toObservable(this.sidePanelContent),
      this._sidePanelService.refresh$,
    ])
      .pipe(takeUntilDestroyed())
      .subscribe(([panelItem, content]) => {
        if (!content) {
          return;
        }

        content.clear();

        if (!panelItem) {
          return;
        }

        const componentRef = content.createComponent(panelItem.component);
        componentRef.instance.data = panelItem.data;
        this._sidePanelService.setPanelTitle(
          panelItem,
          componentRef.instance.title,
        );
      });
  }

  ngOnInit(): void {
    this._sidePanelService.panelAdded
      .pipe(delay(20), takeUntil(this.destroy))
      .subscribe(() => {
        this.dotList()!.nativeElement.scrollTo(
          this.dotList()!.nativeElement.scrollWidth,
          0,
        );
      });
  }

  ngAfterViewInit(): void {
    this._sidePanelService.refresh();
  }

  /**
   * View a previous dot.
   * @param index The index of the dot to view.
   */
  public viewDot(index: number): void {
    if (!this.canNavigate()) {
      return;
    }

    this._sidePanelService.goTo(index);
  }

  public goForward(): void {
    if (!this.canNavigate()) {
      return;
    }

    this._sidePanelService.undo();
  }

  /**
   * Go back to the previous panel.
   */
  public goBack(): void {
    if (!this.canNavigate()) {
      return;
    }

    this._sidePanelService.pop();
  }

  /**
   * Refresh the panel content.
   */
  public refresh(): void {
    this._sidePanelService.refresh();
  }

  /**
   * Close the panel and remove all active panes.
   */
  public close(): void {
    if (!this.canClose()) {
      return;
    }

    this._sidePanelService.clear();
  }

  public get showSidePanel(): Signal<boolean> {
    return this._sidePanelService.showPanel;
  }

  public get sidePanelItems(): Signal<string[]> {
    return this._sidePanelService.sidePanelItems;
  }
}
