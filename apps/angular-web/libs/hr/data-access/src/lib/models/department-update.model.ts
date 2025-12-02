/** Request body for PUT /v1/departments/:id. */
export interface DepartmentUpdate {
  readonly id: number;
  readonly name: string;
  readonly groupName: string;
}
