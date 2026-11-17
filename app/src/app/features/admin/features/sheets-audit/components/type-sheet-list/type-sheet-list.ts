import {
  Component,
  computed,
  effect,
  inject,
  input,
  model,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { InputGroup } from 'primeng/inputgroup';
import { InputGroupAddon } from 'primeng/inputgroupaddon';
import { InputText } from 'primeng/inputtext';
import { SpecSheetPhase } from '../../../../../../core/models/domain/types/spec-sheet-phase';
import { SpecSheetType } from '../../../../../../core/models/domain/types/spec-sheet-type';
import { SheetsAuditService } from '../../sheets-audit.service';

@Component({
  selector: 'app-type-sheet-list',
  imports: [FormsModule, InputText, InputGroup, InputGroupAddon],
  templateUrl: './type-sheet-list.html',
  styleUrl: './type-sheet-list.scss',
})
export class TypeSheetList {
  public readonly activePhase = input.required<SpecSheetPhase>();
  public readonly activeType = model<SpecSheetType | undefined>(undefined);
  public readonly filterText = model<string>('');
  public readonly types = computed(() => {
    const activePhase = this.activePhase();
    const filterText = this.filterText().toLowerCase().trim();
    if (!activePhase) {
      return [];
    }

    if (!filterText || filterText.length === 0) {
      return activePhase.types;
    }

    return activePhase.types.filter((type) =>
      type.type.toLowerCase().includes(filterText),
    );
  });

  private readonly _phaseService = inject(SheetsAuditService);
  private readonly _router = inject(Router);
  private readonly _activatedRoute = inject(ActivatedRoute);

  constructor() {
    effect(() => {
      const activeType = this._phaseService.activeType();
      if (!activeType) {
        this.activeType.set(undefined);
        return;
      }

      this.activeType.set(activeType);
    });
  }

  public selectType(type: SpecSheetType): void {
    this._phaseService.activeType.set(type);

    this._router.navigate([], {
      relativeTo: this._activatedRoute,
      queryParams: { typeId: type.id },
      queryParamsHandling: 'merge',
    });
  }
}
