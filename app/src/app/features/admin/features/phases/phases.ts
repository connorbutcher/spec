import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TreeDragDropService } from 'primeng/api';
import { PhaseList } from './components/phase-list/phase-list';
import { PhaseDataService } from './phase-data.service';
import { PhaseService } from './phase.service';

@Component({
  selector: 'app-phases',
  imports: [PhaseList, RouterOutlet],
  templateUrl: './phases.html',
  styleUrl: './phases.scss',
  providers: [TreeDragDropService, PhaseDataService, PhaseService],
})
export class Phases {}
