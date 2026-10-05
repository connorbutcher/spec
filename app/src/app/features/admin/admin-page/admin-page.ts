import { Component, computed, inject, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TabsModule } from 'primeng/tabs';
import { CurrentUserStore } from '../../../core/auth/current-user.store';
import { CurrentUser } from '../../../core/models/current-user.model';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { AdminPhases } from '../admin-phases/admin-phases';
import { AdminSheetTypes } from '../admin-sheet-types/admin-sheet-types';

/** The admin screen: a tab for the phases and what sheets each has, and a tab for the sheet types. */
@Component({
  selector: 'app-admin-page',
  imports: [AdminPhases, AdminSheetTypes, ButtonModule, EmptyState, TabsModule],
  templateUrl: './admin-page.html',
  styleUrl: './admin-page.scss',
})
export class AdminPage {
  public readonly tab = signal<string | number | undefined>('phases');

  public readonly user = computed<CurrentUser | null>(() => this.currentUser.user());

  /** "Developer · Administrator": who the screen is acting as. */
  public readonly who = computed(() => {
    const user = this.user();
    if (!user) {
      return '';
    }
    const roles = user.roles.length > 0 ? user.roles.join(', ') : 'No roles';
    return `${user.displayName} · ${roles}`;
  });

  private readonly currentUser = inject(CurrentUserStore);

  public hasError(): boolean {
    return this.currentUser.hasError();
  }

  public retry(): void {
    this.currentUser.reload();
  }
}
