import { Component, input } from '@angular/core';
import { SpecSheetType } from '../../../../../core/models/domain/types/spec-sheet-type';
import { PUSpecSheetAvatar } from '../../../../common/pu-spec-sheet-avatar/pu-spec-sheet-avatar';
import { SheetSummaryPropertiesDisplay } from '../../../../properties/components/display/sheet-summary-properties-display/sheet-summary-properties-display';
import { SheetListItem } from '../../../../../core/models/domain/sheets/sheet';
@Component({
  selector: 'app-spec-sheet-version',
  imports: [PUSpecSheetAvatar, SheetSummaryPropertiesDisplay],
  templateUrl: './spec-sheet-version.html',
  styleUrl: './spec-sheet-version.scss',
})
export class SpecSheetVersion {
  public readonly specSheet = input.required<SheetListItem>();
  public readonly specSheetType = input.required<SpecSheetType>();
  public readonly showAvatars = input<boolean>(true);
}
