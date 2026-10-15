import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { GlossaryPage } from './glossary-page';

describe('GlossaryPage', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [GlossaryPage],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  async function render() {
    const fixture = TestBed.createComponent(GlossaryPage);
    fixture.detectChanges();
    return fixture;
  }

  it('lists terms from the API', async () => {
    const fixture = await render();

    http.expectOne('/api/glossary').flush([
      { id: '1', term: 'Coverholder', definition: 'Holds delegated authority.', createdAt: '2026-10-15T00:00:00Z' },
    ]);
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('dt')?.textContent).toContain('Coverholder');
  });

  it('shows a message when the term already exists', async () => {
    const fixture = await render();
    http.expectOne('/api/glossary').flush([]);

    const host = fixture.nativeElement as HTMLElement;
    const term = host.querySelector('input')!;
    const definition = host.querySelector('textarea')!;
    term.value = 'Coverholder';
    term.dispatchEvent(new Event('input'));
    definition.value = 'Holds delegated authority.';
    definition.dispatchEvent(new Event('input'));
    host.querySelector('form')!.dispatchEvent(new Event('submit'));

    const create = http.expectOne({ method: 'POST', url: '/api/glossary' });
    expect(create.request.body).toEqual({ term: 'Coverholder', definition: 'Holds delegated authority.' });
    create.flush(null, { status: 409, statusText: 'Conflict' });
    await fixture.whenStable();

    expect(host.querySelector('[role="alert"]')?.textContent).toContain('already in the glossary');
  });

  it('does not call the API when the form is empty', async () => {
    const fixture = await render();
    http.expectOne('/api/glossary').flush([]);

    (fixture.nativeElement as HTMLElement).querySelector('form')!.dispatchEvent(new Event('submit'));

    http.expectNone({ method: 'POST', url: '/api/glossary' });
  });
});
