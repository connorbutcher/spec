import { Routes } from '@angular/router';

export const TEMPLATES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./templates-page/templates-page').then((m) => m.TemplatesPage),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./template-select-prompt/template-select-prompt').then(
            (m) => m.TemplateSelectPrompt,
          ),
      },
      {
        path: ':templateId',
        loadComponent: () =>
          import('./template-designer/template-designer').then((m) => m.TemplateDesigner),
      },
    ],
  },
];
