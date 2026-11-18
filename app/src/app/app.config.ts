import { OVERLAY_DEFAULT_CONFIG } from '@angular/cdk/overlay';
import {
  ApplicationConfig,
  isDevMode,
  LOCALE_ID,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import {
  provideRouter,
  withComponentInputBinding,
  withInMemoryScrolling,
  withRouterConfig,
} from '@angular/router';

import { DialogModule } from '@angular/cdk/dialog';
import { registerLocaleData } from '@angular/common';
import {
  HTTP_INTERCEPTORS,
  provideHttpClient,
  withInterceptorsFromDi,
} from '@angular/common/http';
import localeEnGb from '@angular/common/locales/en-GB';
import { provideServiceWorker } from '@angular/service-worker';
import { definePreset } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';
import { MessageService } from 'primeng/api';
import { providePrimeNG } from 'primeng/config';
import { routes } from './app.routes';
// import { provideAuth } from './auth/auth.module';
import { LocalUserInterceptor } from './core/interceptors/local-user.interceptor';
import { RequestShareInterceptor } from './core/interceptors/request-share.interceptor';
import { CheckInOutService } from './core/services/check-in-out.service';
import { PhasesService } from './core/services/phases.service';
import { TypesService } from './core/services/types.service';
import { UpdateService } from './features/updater/update.service';

registerLocaleData(localeEnGb);

const AppThemePreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: '{blue.50}',
      100: '{blue.100}',
      200: '{blue.200}',
      300: '{blue.300}',
      400: '{blue.400}',
      500: '{blue.500}',
      600: '{blue.600}',
      700: '{blue.700}',
      800: '{blue.800}',
      900: '{blue.900}',
      950: '{blue.950}',
    },
  },
  components: {
    tooltip: {
      root: {
        maxWidth: 'none',
      },
    },
  },
});

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    // provideAuth(),
    provideHttpClient(withInterceptorsFromDi()),
    provideRouter(
      routes,
      withComponentInputBinding(),
      withRouterConfig({
        paramsInheritanceStrategy: 'always',
        onSameUrlNavigation: 'reload',
      }),
      withInMemoryScrolling({
        anchorScrolling: 'enabled',
      }),
    ),
    providePrimeNG({
      theme: {
        preset: AppThemePreset,
        options: {
          darkModeSelector: '.app-dark',
        },
      },
      license:
        'eyJpZCI6ImFhYmI3YTYzLWRkMjItNGQ3MS04OGI4LTNlMzk3ZWQ2NWM3ZiIsInByb2R1Y3QiOiJwcmltZXVpIiwidGllciI6ImNvbW1lcmNpYWwiLCJ0eXBlIjoiZGV2IiwiaWF0IjoxNzkwMDg3MjM0LCJleHAiOjE4MjE1NjA0MDB9.kp2SrSRaOfdCj-U6FsuA3fl3ZmKbHPqOQX7-yBos9XFAKuvuePmjejM5OXcGq4GzJ7H5kd8nWtaJdCNeb9utAA',
      overlayOptions: {
        autoZIndex: true,
      },
      translation: {
        dateFormat: 'dd/mm/yy',
        firstDayOfWeek: 1,
      },
    }),
    {
      provide: LOCALE_ID,
      useValue: 'en-GB',
    },
    {
      provide: OVERLAY_DEFAULT_CONFIG,
      useValue: { usePopover: false },
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: LocalUserInterceptor,
      multi: true,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: RequestShareInterceptor,
      multi: true,
    },
    DialogModule,
    MessageService,
    TypesService,
    PhasesService,
    CheckInOutService,
    UpdateService,
    provideServiceWorker('ngsw-worker.js', {
      enabled: !isDevMode(),
      registrationStrategy: 'registerWhenStable:30000',
    }),
  ],
};
