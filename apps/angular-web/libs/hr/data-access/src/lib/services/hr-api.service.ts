import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@adventureworks-web/shared/util';
import type { Department, SearchResult } from '@adventureworks-web/shared/data-access';
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
import type { DepartmentCreate } from '../models/department-create.model';
import type { DepartmentUpdate } from '../models/department-update.model';
import type { DepartmentHeadcount } from '../models/department-headcount.model';
import type { DepartmentEmployeesParams } from '../models/department-employees-params.model';

/** HTTP client for HR domain endpoints (Employees, Employee Lifecycle, and Departments). */
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

  getDepartments(): Observable<Department[]> {
    return this.apiService.get<Department[]>('/v1/departments');
  }

  getDepartment(id: number): Observable<Department> {
    return this.apiService.get<Department>(`/v1/departments/${id}`);
  }

  createDepartment(model: DepartmentCreate): Observable<Department> {
    return this.apiService.post<Department>('/v1/departments', model);
  }

  updateDepartment(id: number, model: DepartmentUpdate): Observable<Department> {
    return this.apiService.put<Department>(`/v1/departments/${id}`, model);
  }

  getDepartmentHeadcount(id: number): Observable<DepartmentHeadcount> {
    return this.apiService.get<DepartmentHeadcount>(`/v1/departments/${id}/headcount`);
  }

  getDepartmentEmployees(id: number, params?: DepartmentEmployeesParams): Observable<Employee[]> {
    const query = params ? toQueryString(params) : '';
    return this.apiService.get<Employee[]>(`/v1/departments/${id}/employees${query}`);
  }
}
