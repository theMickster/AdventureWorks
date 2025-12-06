// Models
export type { Employee } from './lib/models/employee.model';
export type { EmployeeCreate, EmployeeAddressCreate } from './lib/models/employee-create.model';
export type { EmployeeUpdate } from './lib/models/employee-update.model';
export type { EmployeeParams } from './lib/models/employee-params.model';
export type { EmployeeSearchBody } from './lib/models/employee-search.model';
export type {
  EmployeeHire,
  EmployeeTerminate,
  EmployeeRehire,
  EmployeeLifecycleStatus,
} from './lib/models/employee-lifecycle.model';
export type { JsonPatchOperation } from './lib/models/json-patch.model';
export { SHIFT_OPTIONS, PAY_FREQUENCY_OPTIONS, TERMINATION_TYPE_OPTIONS } from './lib/models/employee-lifecycle-options.model';
export type { DepartmentCreate } from './lib/models/department-create.model';
export type { DepartmentUpdate } from './lib/models/department-update.model';
export type { DepartmentHeadcount } from './lib/models/department-headcount.model';
export type { DepartmentEmployeesParams } from './lib/models/department-employees-params.model';

// Services
export { HrApiService } from './lib/services/hr-api.service';

// Stores
export { EmployeeStore } from './lib/stores/employee.store';
