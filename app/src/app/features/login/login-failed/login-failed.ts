import { CommonModule } from '@angular/common';
import { Component, input, OnInit, signal } from '@angular/core';
import { MsalService } from '@azure/msal-angular';

type Context = {
  timestamp: Date;
  userObjectId: string;
  username: string;
};

@Component({
  selector: 'app-login-failed',
  imports: [CommonModule],
  templateUrl: './login-failed.html',
  styleUrl: './login-failed.scss',
})
export class LoginFailed implements OnInit {
  public readonly reason = input.required<string>();
  public readonly context = signal<Context | undefined>(undefined);

  constructor(private readonly _msalService: MsalService) {}

  ngOnInit(): void {
    this.context.set(this.generateContext());
  }

  private generateContext(): Context {
    const activeAccount = this._msalService.instance.getActiveAccount();
    const userObjectId = activeAccount?.idTokenClaims?.oid;

    return {
      timestamp: new Date(),
      userObjectId: userObjectId ?? 'unknown',
      username: activeAccount?.username ?? 'unknown',
    };
  }
}
