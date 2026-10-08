import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should render the app name and side nav', () => {
    const fixture = TestBed.createComponent(App);
    // Not whenStable(): the nav's pending health-check request would keep it waiting.
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('app-side-nav .brand')?.textContent).toContain('PU Spec Sheet');
    const navLabels = Array.from(compiled.querySelectorAll('.side-nav-link .label')).map((el) =>
      el.textContent?.trim(),
    );
    expect(navLabels).toEqual(['Phases', 'Templates', 'Admin', 'Help']);
  });
});
