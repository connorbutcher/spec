import { computed, Injectable, Signal, signal, Type } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { ISidePanelLayer } from './models/side-panel-layer-data.interface';
import { ISidePanelLayerItem } from './models/side-panel-layer-item.interface';
import { Stack } from './models/stack.class';

@Injectable({
  providedIn: 'root',
})
export class SidePanelService {
  public readonly showPanel = signal<boolean>(false);
  public readonly activePanelId = signal<string>('');
  public readonly activePanel = signal<ISidePanelLayerItem<
    ISidePanelLayer<any>
  > | null>(null);
  public readonly refresh$ = new BehaviorSubject<void>(undefined);

  private readonly _componentStack = new Stack<
    ISidePanelLayerItem<ISidePanelLayer<any>>
  >();
  private readonly _undoComponentStack = new Stack<
    ISidePanelLayerItem<ISidePanelLayer<any>>
  >();
  private readonly _panelItems = signal<
    ISidePanelLayerItem<ISidePanelLayer<any>>[]
  >([]);
  private readonly _panelAdded$ = new Subject<void>();
  private readonly _panelRemoved$ = new Subject<void>();

  public readonly sidePanelItems = computed(() =>
    this._panelItems().map((item) => item.title?.() ?? ''),
  );

  /**
   * Push a panel component onto the stack and render it.
   * @param component The component to display in the panel.
   * @param data The data associated with the component.
   */
  public push<C extends ISidePanelLayer<D>, D>(
    component: Type<C>,
    data: D,
    id: string,
  ): void {
    this._componentStack.push({ component, data, id });
    this._panelItems.set([...this._componentStack.getAll()]);
    this.setActivePanel();

    this.setShowPanel(true);
    this._panelAdded$.next();
  }

  /**
   * Pop a panel component of the stack.
   */
  public pop(): void {
    const removedItem = this._componentStack.pop();
    this._panelItems.set([...this._componentStack.getAll()]);
    if (removedItem) {
      this._undoComponentStack.push(removedItem);
    }

    if (this._componentStack.size === 0) {
      this.setShowPanel(false);
      this.activePanel.set(null);
    } else {
      this.setActivePanel();
    }

    this._panelAdded$.next();
  }

  public undo(): void {
    if (this._undoComponentStack.size === 0) {
      return;
    }

    const item = this._undoComponentStack.pop();
    if (item) {
      this.push(item.component, item.data, item.id);
    }
  }

  /**
   * Clear all components from the stack.
   */
  public clear(): void {
    while (this._componentStack.size > 0) {
      this.pop();
    }

    while (this._undoComponentStack.size > 0) {
      this._undoComponentStack.pop();
    }

    this.activePanelId.set('');
  }

  /**
   * View a pane at a specified index.
   * @param index The index of the pane to view.
   */
  public goTo(index: number): void {
    for (let i = this._componentStack.size - 1; i >= 0; i--) {
      if (i === index) {
        this.setActivePanel();
        return;
      }

      const removedItem = this._componentStack.pop();
      this._panelItems.set([...this._componentStack.getAll()]);
      if (removedItem) {
        this._undoComponentStack.push(removedItem);
      }
    }
  }

  public refresh(): void {
    this.refresh$.next();
  }

  public setShowPanel(show: boolean): void {
    this.showPanel.set(show);
  }

  public get panelAdded(): Observable<void> {
    return this._panelAdded$.asObservable();
  }

  public setPanelTitle(
    item: ISidePanelLayerItem<ISidePanelLayer<any>>,
    title: Signal<string>,
  ): void {
    item.title = title;
    this._panelItems.set([...this._componentStack.getAll()]);
  }

  private setActivePanel(): void {
    const sidePanelLayerItem = this._componentStack.peek();
    if (!sidePanelLayerItem) {
      this.activePanel.set(null);
      this.activePanelId.set('');
      return;
    }

    this.activePanel.set(sidePanelLayerItem);
    this.activePanelId.set(sidePanelLayerItem.id);
  }
}
