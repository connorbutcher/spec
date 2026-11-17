import { Component } from '@angular/core';
import { Message } from 'primeng/message';
import { EditProperties } from '../../../../../../../properties/components/manage/edit-properties/edit-properties';

@Component({
  selector: 'app-update-spec-sheet-type-properties',
  imports: [EditProperties, Message],
  templateUrl: './update-spec-sheet-type-properties.html',
  styleUrl: './update-spec-sheet-type-properties.scss',
})
export class UpdateSpecSheetTypeProperties {}
