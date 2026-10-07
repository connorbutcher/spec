import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { providePrimeNG } from 'primeng/config';
import { routes } from './app.routes';
import { developerUserInterceptor } from './core/auth/developer-user.interceptor';
import { sheetConnectionInterceptor } from './features/sheets/sheet-connection.interceptor';
import { SpecSheetPreset } from './theme';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(
      withFetch(),
      withInterceptors([developerUserInterceptor, sheetConnectionInterceptor]),
    ),
    providePrimeNG({
      theme: {
        preset: SpecSheetPreset,
        options: {
          darkModeSelector: '.app-dark',
        },
      },
    }),
  ],
};
