import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { correlationIdInterceptor } from './correlation-id.interceptor';

const UUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

describe('correlationIdInterceptor', () => {
  let httpClient: HttpClient;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(withInterceptors([correlationIdInterceptor])), provideHttpClientTesting()],
    });

    httpClient = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should send an X-Correlation-ID UUID header on every request method', () => {
    httpClient.get('/api/test').subscribe();
    httpClient.post('/api/test', {}).subscribe();
    httpClient.put('/api/test', {}).subscribe();
    httpClient.patch('/api/test', {}).subscribe();
    httpClient.delete('/api/test').subscribe();

    const requests = httpTesting.match('/api/test');

    expect(requests.map((r) => r.request.method)).toEqual(['GET', 'POST', 'PUT', 'PATCH', 'DELETE']);
    for (const testRequest of requests) {
      expect(testRequest.request.headers.get('X-Correlation-ID')).toMatch(UUID_PATTERN);
      testRequest.flush({});
    }
  });

  it('should generate a distinct correlation id for each request', () => {
    httpClient.get('/api/first').subscribe();
    httpClient.get('/api/second').subscribe();

    const first = httpTesting.expectOne('/api/first');
    const second = httpTesting.expectOne('/api/second');

    expect(first.request.headers.get('X-Correlation-ID')).not.toBe(second.request.headers.get('X-Correlation-ID'));

    first.flush({});
    second.flush({});
  });
});
