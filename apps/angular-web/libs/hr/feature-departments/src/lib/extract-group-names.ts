import type { Department } from '@adventureworks-web/shared/data-access';

/** Distinct group names from a department list, sorted alphabetically. */
export function extractGroupNames(departments: Department[]): string[] {
  return [...new Set(departments.map((d) => d.groupName))].sort((a, b) => a.localeCompare(b));
}
