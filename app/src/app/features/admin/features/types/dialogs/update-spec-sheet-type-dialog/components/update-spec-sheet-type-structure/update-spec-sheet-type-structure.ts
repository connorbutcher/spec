import { Component } from '@angular/core';
import { Message } from 'primeng/message';
import { SpecSheetTypeStructure } from '../../../../../../../structure/components/spec-sheet-type-structure/spec-sheet-type-structure';

@Component({
  selector: 'app-update-spec-sheet-type-structure',
  imports: [SpecSheetTypeStructure, Message],
  templateUrl: './update-spec-sheet-type-structure.html',
  styleUrl: './update-spec-sheet-type-structure.scss',
})
export class UpdateSpecSheetTypeStructure {}
