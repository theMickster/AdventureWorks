import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { LoadingService } from '../loading/loading.service';
import { loadingInterceptor } from './loading.interceptor';

describe('loadingInterceptor', () => {
  let httpClient: HttpClient;
  let httpTesting: HttpTestingController;
  let loadingService: LoadingService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(withInterceptors([loadingInterceptor])), provideHttpClientTesting()],
    });

    httpClient = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
    loadingService = TestBed.inject(LoadingService);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should call start on request and stop on success', () => {
    const startSpy = vi.spyOn(loadingService, 'start');
    const stopSpy = vi.spyOn(loadingService, 'stop');

    httpClient.get('/api/test').subscribe();

    expect(startSpy).toHaveBeenCalledTimes(1);
    expect(stopSpy).not.toHaveBeenCalled();

    httpTesting.expectOne('/api/test').flush({ data: 'ok' });

    expect(stopSpy).toHaveBeenCalledTimes(1);
  });

  it('should call stop on HTTP error via finalize', () => {
    const stopSpy = vi.spyOn(loadingService, 'stop');

    httpClient.get('/api/test').subscribe({
      error: () => {
        /* expected */
      },
    });

    httpTesting.expectOne('/api/test').flush('Server Error', { status: 500, statusText: 'Internal Server Error' });

    expect(stopSpy).toHaveBeenCalledTimes(1);
  });

  it('should skip loading for X-Silent-Request header', () => {
    const startSpy = vi.spyOn(loadingService, 'start');
    const stopSpy = vi.spyOn(loadingService, 'stop');

    httpClient.get('/api/test', { headers: { 'X-Silent-Request': 'true' } }).subscribe();

    expect(startSpy).not.toHaveBeenCalled();

    httpTesting.expectOne('/api/test').flush({ data: 'ok' });

    expect(stopSpy).not.toHaveBeenCalled();
  });

  it('should strip X-Silent-Request header from outgoing request', () => {
    httpClient.get('/api/test', { headers: { 'X-Silent-Request': 'true' } }).subscribe();

    const req = httpTesting.expectOne('/api/test');
    expect(req.request.headers.has('X-Silent-Request')).toBe(false);

    req.flush({ data: 'ok' });
  });
});
