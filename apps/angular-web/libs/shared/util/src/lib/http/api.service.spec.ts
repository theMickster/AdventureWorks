import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ENVIRONMENT } from '../environment/environment.token';
import { Environment } from '../environment/environment.model';
import { ApiService } from './api.service';

const mockEnvironment: Environment = {
  production: false,
  defaultLocale: 'en',
  api: {
    primary: { baseUrl: 'https://localhost:5001/api', name: 'Primary API' },
    functions: { baseUrl: 'https://func.azurewebsites.net/api', name: 'Azure Functions' },
  },
};

describe('ApiService', () => {
  let service: ApiService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: ENVIRONMENT, useValue: mockEnvironment }],
    });

    service = TestBed.inject(ApiService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should use primary baseUrl by default for get', () => {
    service.get('/users').subscribe();
    const req = httpTesting.expectOne('https://localhost:5001/api/users');
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should resolve named API key for get', () => {
    service.get('/status', 'functions').subscribe();
    const req = httpTesting.expectOne('https://func.azurewebsites.net/api/status');
    expect(req.request.method).toBe('GET');
    req.flush({ ok: true });
  });

  it('should throw for nonexistent API key', () => {
    expect(() => service.get('/test', 'nonexistent').subscribe()).toThrowError(
      /API key "nonexistent" not found.*Available keys: primary, functions/,
    );
  });

  it('should use primary baseUrl by default for post', () => {
    service.post('/users', { name: 'Test' }).subscribe();
    const req = httpTesting.expectOne('https://localhost:5001/api/users');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ name: 'Test' });
    req.flush({});
  });

  it('should resolve named API key for post', () => {
    service.post('/process', { id: 1 }, 'functions').subscribe();
    const req = httpTesting.expectOne('https://func.azurewebsites.net/api/process');
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should use primary baseUrl by default for put', () => {
    service.put('/users/1', { name: 'Updated' }).subscribe();
    const req = httpTesting.expectOne('https://localhost:5001/api/users/1');
    expect(req.request.method).toBe('PUT');
    req.flush({});
  });

  it('should use primary baseUrl by default for patch', () => {
    service.patch('/users/1', { name: 'Patched' }).subscribe();
    const req = httpTesting.expectOne('https://localhost:5001/api/users/1');
    expect(req.request.method).toBe('PATCH');
    req.flush({});
  });

  it('should use primary baseUrl by default for delete', () => {
    service.delete('/users/1').subscribe();
    const req = httpTesting.expectOne('https://localhost:5001/api/users/1');
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('should resolve named API key for delete', () => {
    service.delete('/cache/clear', 'functions').subscribe();
    const req = httpTesting.expectOne('https://func.azurewebsites.net/api/cache/clear');
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
