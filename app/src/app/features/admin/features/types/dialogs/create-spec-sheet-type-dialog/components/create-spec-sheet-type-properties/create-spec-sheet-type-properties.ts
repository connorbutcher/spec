import { Component } from '@angular/core';
import { Message } from 'primeng/message';
import { EditProperties } from '../../../../../../../properties/components/manage/edit-properties/edit-properties';

@Component({
  selector: 'app-create-spec-sheet-type-properties',
  imports: [EditProperties, Message],
  templateUrl: './create-spec-sheet-type-properties.html',
  styleUrl: './create-spec-sheet-type-properties.scss',
})
export class CreateSpecSheetTypeProperties {}
