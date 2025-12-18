/** HumanResources.Department.GroupName (6 distinct values) mapped to DaisyUI semantic color classes. */
const GROUP_NAME_TO_COLOR_CLASS: Readonly<Record<string, string>> = {
  'Executive General and Administration': 'primary',
  'Sales and Marketing': 'accent',
  'Research and Development': 'info',
  Manufacturing: 'warning',
  'Inventory Management': 'secondary',
  'Quality Assurance': 'success',
};

/** Fallback DaisyUI class for a group name outside the 6 known values (e.g. an unassigned employee's blank department). */
const DEFAULT_COLOR_CLASS = 'neutral';

export function resolveGroupColorClass(groupName: string | null | undefined): string {
  if (!groupName) {
    return DEFAULT_COLOR_CLASS;
  }

  return GROUP_NAME_TO_COLOR_CLASS[groupName] ?? DEFAULT_COLOR_CLASS;
}
