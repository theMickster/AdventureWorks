import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ENVIRONMENT } from '../environment/environment.token';

/** Typed HTTP client wrapper that prepends the primary API base URL to all requests. */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(ENVIRONMENT).api.primary.baseUrl;

  /** GET {baseUrl}/{path} */
  get<T>(path: string): Observable<T> {
    return this.http.get<T>(`${this.baseUrl}${path}`);
  }

  /** POST {baseUrl}/{path} with body */
  post<T>(path: string, body: unknown): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}${path}`, body);
  }

  /** PUT {baseUrl}/{path} with body */
  put<T>(path: string, body: unknown): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}${path}`, body);
  }

  /** PATCH {baseUrl}/{path} with body */
  patch<T>(path: string, body: unknown): Observable<T> {
    return this.http.patch<T>(`${this.baseUrl}${path}`, body);
  }

  /** DELETE {baseUrl}/{path} */
  delete(path: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}${path}`);
  }
}
