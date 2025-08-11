import type { PaginationParams } from '@adventureworks-web/shared/data-access';

/** Query parameters for GET /v1/employees. Extends shared pagination with employee-specific sort options. */
export interface EmployeeParams extends PaginationParams {
  readonly orderBy?: 'id' | 'firstName' | 'lastName';
}
