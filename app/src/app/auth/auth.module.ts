import { PlatformLocation } from '@angular/common';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import {
  EnvironmentProviders,
  importProvidersFrom,
  makeEnvironmentProviders,
} from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import {
  MSAL_GUARD_CONFIG,
  MSAL_INSTANCE,
  MSAL_INTERCEPTOR_CONFIG,
  MsalBroadcastService,
  MsalGuard,
  MsalGuardConfiguration,
  MsalInterceptor,
  MsalInterceptorConfiguration,
  MsalService,
} from '@azure/msal-angular';
import {
  BrowserCacheLocation,
  InteractionType,
  IPublicClientApplication,
  LogLevel,
  PublicClientApplication,
  RedirectRequest,
} from '@azure/msal-browser';
import { environment } from '../../../environments/environment';

function loggerCallback(logLevel: LogLevel, message: string) {
  //console.log(message);
}

function MSALInstanceFactory(
  platformLocation: PlatformLocation,
): IPublicClientApplication {
  let baseHref = platformLocation.getBaseHrefFromDOM();

  //remove trailing slash
  const baseHrefLength = baseHref.length;
  if (baseHrefLength > 0 && baseHref[baseHrefLength - 1] === '/') {
    baseHref = baseHref.substring(0, baseHrefLength - 1);
  }

  return new PublicClientApplication({
    auth: {
      clientId: environment.auth.clientId,
      authority: environment.auth.authority,
      redirectUri: `${baseHref}/login/oauth-callback`,
      postLogoutRedirectUri: `${baseHref}/logout`,
      navigateToLoginRequestUrl: false,
    },
    cache: {
      cacheLocation: BrowserCacheLocation.SessionStorage,
    },
    system: {
      allowPlatformBroker: false, // Disables WAM Broker
      loggerOptions: {
        loggerCallback,
        logLevel: LogLevel.Info,
        piiLoggingEnabled: false,
      },
    },
  });
}

function MSALInterceptorConfigFactory(): MsalInterceptorConfiguration {
  const protectedResourceMap = new Map<string, Array<string>>(
    Object.entries(environment.auth.resourceMap),
  );

  return {
    interactionType: InteractionType.Redirect,
    protectedResourceMap,
  };
}

function MSALGuardConfigFactory(): MsalGuardConfiguration {
  return {
    interactionType: InteractionType.Redirect,
    authRequest: (authService, state) => {
      const redirectUri =
        window.location.pathname +
        window.location.search +
        window.location.hash;
      const authState = { postLoginRedirectUri: redirectUri };

      return {
        state: btoa(JSON.stringify(authState)),
      } as RedirectRequest;
    },
    loginFailedRoute: '/login-failed',
  };
}

export function provideAuth(): EnvironmentProviders {
  return makeEnvironmentProviders([
    importProvidersFrom(BrowserModule),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: MsalInterceptor,
      multi: true,
    },
    {
      provide: MSAL_INSTANCE,
      useFactory: (pl: PlatformLocation) => MSALInstanceFactory(pl),
      deps: [PlatformLocation],
    },
    {
      provide: MSAL_GUARD_CONFIG,
      useFactory: MSALGuardConfigFactory,
    },
    {
      provide: MSAL_INTERCEPTOR_CONFIG,
      useFactory: MSALInterceptorConfigFactory,
    },
    MsalService,
    MsalGuard,
    MsalBroadcastService,
  ]);
}
