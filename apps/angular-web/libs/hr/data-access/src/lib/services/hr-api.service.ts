import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@adventureworks-web/shared/util';
import type { SearchResult } from '@adventureworks-web/shared/data-access';
import { toQueryString } from '@adventureworks-web/shared/data-access';
import type { Employee } from '../models/employee.model';
import type { EmployeeCreate } from '../models/employee-create.model';
import type { EmployeeUpdate } from '../models/employee-update.model';
import type { EmployeeParams } from '../models/employee-params.model';
import type { EmployeeSearchBody } from '../models/employee-search.model';
import type {
  EmployeeHire,
  EmployeeTerminate,
  EmployeeRehire,
  EmployeeLifecycleStatus,
} from '../models/employee-lifecycle.model';
import type { JsonPatchOperation } from '../models/json-patch.model';

/** HTTP client for HR domain endpoints (Employees and Employee Lifecycle). */
@Injectable({ providedIn: 'root' })
export class HrApiService {
  private readonly apiService = inject(ApiService);

  getEmployee(id: number): Observable<Employee> {
    return this.apiService.get<Employee>(`/v1/employees/${id}`);
  }

  getEmployees(params?: EmployeeParams): Observable<SearchResult<Employee>> {
    const query = params ? toQueryString(params) : '';
    return this.apiService.get<SearchResult<Employee>>(`/v1/employees${query}`);
  }

  searchEmployees(params: EmployeeParams, body: EmployeeSearchBody): Observable<SearchResult<Employee>> {
    const query = params ? toQueryString(params) : '';
    return this.apiService.post<SearchResult<Employee>>(`/v1/employees/search${query}`, body);
  }

  createEmployee(model: EmployeeCreate): Observable<Employee> {
    return this.apiService.post<Employee>('/v1/employees', model);
  }

  updateEmployee(id: number, model: EmployeeUpdate): Observable<Employee> {
    return this.apiService.put<Employee>(`/v1/employees/${id}`, model);
  }

  patchEmployee(id: number, operations: JsonPatchOperation[]): Observable<Employee> {
    return this.apiService.patch<Employee>(`/v1/employees/${id}`, operations);
  }

  hireEmployee(id: number, model: EmployeeHire): Observable<{ businessEntityId: number; message: string }> {
    return this.apiService.post<{ businessEntityId: number; message: string }>(
      `/v1/employees/${id}/lifecycle/hire`,
      model,
    );
  }

  terminateEmployee(id: number, model: EmployeeTerminate): Observable<{ message: string }> {
    return this.apiService.post<{ message: string }>(`/v1/employees/${id}/lifecycle/terminate`, model);
  }

  rehireEmployee(id: number, model: EmployeeRehire): Observable<{ businessEntityId: number; message: string }> {
    return this.apiService.post<{ businessEntityId: number; message: string }>(
      `/v1/employees/${id}/lifecycle/rehire`,
      model,
    );
  }

  getLifecycleStatus(id: number): Observable<EmployeeLifecycleStatus> {
    return this.apiService.get<EmployeeLifecycleStatus>(`/v1/employees/${id}/lifecycle/status`);
  }
}
