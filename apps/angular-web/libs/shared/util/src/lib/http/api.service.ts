import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Environment } from '../environment/environment.model';
import { ENVIRONMENT } from '../environment/environment.token';

/** Typed HTTP client wrapper that resolves API base URLs from the environment registry. */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly environment = inject<Environment>(ENVIRONMENT);

  /** GET {baseUrl}/{path} */
  get<T>(path: string, apiKey = 'primary'): Observable<T> {
    return this.http.get<T>(`${this.resolveBaseUrl(apiKey)}${path}`);
  }

  /** POST {baseUrl}/{path} with body */
  post<T>(path: string, body: unknown, apiKey = 'primary'): Observable<T> {
    return this.http.post<T>(`${this.resolveBaseUrl(apiKey)}${path}`, body);
  }

  /** PUT {baseUrl}/{path} with body */
  put<T>(path: string, body: unknown, apiKey = 'primary'): Observable<T> {
    return this.http.put<T>(`${this.resolveBaseUrl(apiKey)}${path}`, body);
  }

  /** PATCH {baseUrl}/{path} with body */
  patch<T>(path: string, body: unknown, apiKey = 'primary'): Observable<T> {
    return this.http.patch<T>(`${this.resolveBaseUrl(apiKey)}${path}`, body);
  }

  /** DELETE {baseUrl}/{path} */
  delete(path: string, apiKey = 'primary'): Observable<void> {
    return this.http.delete<void>(`${this.resolveBaseUrl(apiKey)}${path}`);
  }

  private resolveBaseUrl(apiKey: string): string {
    const endpoint = this.environment.api[apiKey];
    if (!endpoint) {
      const available = Object.keys(this.environment.api).join(', ');
      throw new Error(`API key "${apiKey}" not found in environment. Available keys: ${available}`);
    }
    return endpoint.baseUrl;
  }
}
