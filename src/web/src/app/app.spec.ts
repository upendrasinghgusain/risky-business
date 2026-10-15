import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';

describe('App', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('shows the API as healthy when the readiness check succeeds', async () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();

    http.expectOne('/health/ready').flush('Healthy');
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('.status')?.textContent).toContain('healthy');
  });

  it('shows the API as unavailable when the readiness check fails', async () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();

    http
      .expectOne('/health/ready')
      .flush('Unhealthy', { status: 503, statusText: 'Service Unavailable' });
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('.status')?.textContent).toContain('unavailable');
  });
});
