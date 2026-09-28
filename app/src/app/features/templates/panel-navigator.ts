import { computed, inject, Injectable, signal } from '@angular/core';
import { PanelRef } from './models/panel-ref';
import { panelCrumbs } from './panel-crumbs.util';
import { TemplatesStore } from './templates.store';

/**
 * The configuration panel's history, like a browser's: opening something pushes it and clears the
 * forward steps; back and forward move through what was opened. Provided by the templates page,
 * alongside the store it checks after deletes.
 */
@Injectable()
export class PanelNavigator {
  public readonly current = computed<PanelRef>(() => this.history()[this.index()]);
  public readonly canGoBack = computed(() => this.index() > 0);
  public readonly canGoForward = computed(() => this.index() < this.history().length - 1);

  private readonly store = inject(TemplatesStore);
  private readonly history = signal<PanelRef[]>([{ kind: 'cellTypes' }]);
  private readonly index = signal(0);

  /** Shows `ref`, unless it's already showing. */
  public open(ref: PanelRef): void {
    if (samePanel(ref, this.current())) {
      return;
    }
    const kept = this.history().slice(0, this.index() + 1);
    this.history.set([...kept, ref]);
    this.index.set(kept.length);
  }

  /** Starts a fresh history at `ref`. */
  public reset(ref: PanelRef): void {
    this.history.set([ref]);
    this.index.set(0);
  }

  public back(): void {
    if (this.canGoBack()) {
      this.index.update((index) => index - 1);
    }
  }

  public forward(): void {
    if (this.canGoForward()) {
      this.index.update((index) => index + 1);
    }
  }

  /**
   * Drops history steps for things that no longer exist (after a delete), then shows `fallback` if
   * the current step went with them.
   */
  public forget(isGone: (ref: PanelRef) => boolean, fallback: PanelRef): void {
    const history = this.history();
    const index = this.index();

    if (isGone(history[index])) {
      const steps = withoutRepeats([
        ...history.slice(0, index).filter((ref) => !isGone(ref)),
        fallback,
      ]);
      this.history.set(steps);
      this.index.set(steps.length - 1);
      return;
    }

    // Keep the current step current: count the surviving steps before it.
    const before = withoutRepeats(history.slice(0, index + 1).filter((ref) => !isGone(ref)));
    const after = history.slice(index + 1).filter((ref) => !isGone(ref));
    const steps = withoutRepeats([...before, ...after]);
    this.history.set(steps);
    this.index.set(before.length - 1);
  }

  /** After a delete: drops steps for anything that's gone and shows `fallback` if the current one went. */
  public forgetMissing(fallback: PanelRef): void {
    const template = this.store.template();
    const index = this.store.index();
    this.forget(
      (ref) =>
        ref.kind !== 'template' &&
        panelCrumbs(ref, template, index, (id) => this.store.cellType(id)).length === 0,
      fallback,
    );
  }

  public isShowing(ref: PanelRef): boolean {
    return samePanel(ref, this.current());
  }
}

/** Collapses back-to-back steps for the same panel into one. */
function withoutRepeats(steps: PanelRef[]): PanelRef[] {
  return steps.filter((ref, index) => index === 0 || !samePanel(ref, steps[index - 1]));
}

function samePanel(a: PanelRef, b: PanelRef): boolean {
  if (a.kind !== b.kind) {
    return false;
  }
  return !('id' in a) || !('id' in b) || a.id === b.id;
}
