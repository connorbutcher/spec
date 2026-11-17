import { PlatformLocation } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MsalService } from '@azure/msal-angular';
import { AuthenticationResult } from '@azure/msal-browser';
import { lastValueFrom, take } from 'rxjs';
import { CheckInOutService } from '../../../core/services/check-in-out.service';
import { UserService } from '../../../core/services/user.service';
import { Spinner } from '../../common/spinner/spinner';
import { OAuthCallbackDataService } from './oauth-callback-data.service';

@Component({
  selector: 'app-oauth-callback',
  imports: [Spinner],
  templateUrl: './oauth-callback.html',
  styleUrl: './oauth-callback.scss',
  providers: [OAuthCallbackDataService],
})
export class OAuthCallback implements OnInit {
  private readonly _authService = inject(MsalService);
  private readonly _router = inject(Router);
  private readonly _activatedRoute = inject(ActivatedRoute);
  private readonly _platformLocation = inject(PlatformLocation);
  private readonly _oauthCallbackDataService = inject(OAuthCallbackDataService);
  private readonly _userService = inject(UserService);
  private readonly _checkInOutService = inject(CheckInOutService);

  public ngOnInit(): void {
    this.initAsync();
  }

  private async initAsync() {
    await lastValueFrom(this._authService.initialize());
    const result = await this._authService.instance.handleRedirectPromise();

    if (result) {
      this.checkAndSetActiveAccount();
      this.handleAuthRedirectAsync(result);
    } else {
      const paramMap = await lastValueFrom(
        this._activatedRoute.queryParamMap.pipe(take(1)),
      );
      const postLoginRoute = paramMap.get('postLoginRoute') ?? '/';
      this.attemptLoginAsync(decodeURIComponent(postLoginRoute));
    }
  }

  private async handleAuthRedirectAsync(result: AuthenticationResult) {
    const state = result.state;

    if (!state) {
      this.failLogin('idp_no-state');
      return;
    }

    const decodedState = atob(state);
    const json = JSON.parse(decodedState);
    const postLoginUri: string | undefined = json.postLoginRedirectUri;

    if (!postLoginUri) {
      this.failLogin('idp_no-post-login-uri');
      return;
    }

    this.attemptLoginAsync(postLoginUri);
  }

  private async attemptLoginAsync(postLoginRoute: string) {
    postLoginRoute = this.sanitizePostLoginRoute(postLoginRoute);

    const account = this._authService.instance.getActiveAccount();
    if (!account) {
      this.failLogin('idp_no-active-account');
      return;
    }

    try {
      const user = await this._oauthCallbackDataService.getCurrentUserAsync();
      if (!user) {
        this.failLogin('api_user-incorrect-setup');
        return;
      }

      this._userService.setUser(user);
      await this._checkInOutService.startConnectionAsync(
        user.displayName,
        user.objectId,
      );

      this._router.navigateByUrl(postLoginRoute, { replaceUrl: true });
    } catch (e) {
      if (e instanceof HttpErrorResponse) {
        switch (e.status) {
          case 0:
            this.failLogin('client_error');
            return;
          case 401:
            this.failLogin('api_user-unauthorized');
            return;
          case 403:
            this.failLogin('api_user-forbidden');
            return;
          case 500:
            this.failLogin('api_internal-error');
            return;
        }
      }

      this.failLogin('unknown_error');
    }
  }

  private sanitizePostLoginRoute(postLoginRoute: string): string {
    // If the baseHref is not '/', remove it from the postLoginRoute
    const baseHref = this._platformLocation.getBaseHrefFromDOM();
    if (baseHref !== '/' && postLoginRoute.startsWith(baseHref)) {
      postLoginRoute = postLoginRoute.substring(baseHref.length);
    }

    // Don't allow the user to redirect to the login page
    if (postLoginRoute.endsWith('callback')) {
      postLoginRoute = '/';
    }

    return postLoginRoute;
  }

  private failLogin(reason: string) {
    this._router.navigate(['/login-failed'], {
      queryParams: { reason },
      skipLocationChange: true,
    });
  }

  private checkAndSetActiveAccount() {
    let activeAccount = this._authService.instance.getActiveAccount();

    if (
      !activeAccount &&
      this._authService.instance.getAllAccounts().length > 0
    ) {
      let accounts = this._authService.instance.getAllAccounts();
      this._authService.instance.setActiveAccount(accounts[0]);
    }
  }
}
