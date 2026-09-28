import { Component } from '@angular/core';

@Component({
  selector: 'app-home-page',
  template: `
    <section class="app-card home">
      <h1>PU Spec Sheet</h1>
      <p>Sections, templates and limit tables will live here.</p>
    </section>
  `,
  styles: `
    .home {
      max-width: 720px;
      padding: 24px;

      h1 {
        margin: 0 0 8px;
        font-size: 1.4rem;
        color: var(--app-navy);
      }

      p {
        margin: 0;
        color: #64748b;
      }
    }
  `,
})
export class HomePageComponent {}
