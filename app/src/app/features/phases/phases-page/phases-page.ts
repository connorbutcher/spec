import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

/** The phases screen: the selected phase, full width. The phase tree lives in the side nav. */
@Component({
  selector: 'app-phases-page',
  imports: [RouterOutlet],
  templateUrl: './phases-page.html',
  styleUrl: './phases-page.scss',
})
export class PhasesPage {}
