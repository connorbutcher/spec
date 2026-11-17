import { Component } from '@angular/core';
import { Message } from 'primeng/message';
import { SpecSheetTypeStructure } from '../../../../../../../structure/components/spec-sheet-type-structure/spec-sheet-type-structure';

@Component({
  selector: 'app-create-spec-sheet-type-structure',
  imports: [SpecSheetTypeStructure, Message],
  templateUrl: './create-spec-sheet-type-structure.html',
  styleUrl: './create-spec-sheet-type-structure.scss',
})
export class CreateSpecSheetTypeStructure {}
