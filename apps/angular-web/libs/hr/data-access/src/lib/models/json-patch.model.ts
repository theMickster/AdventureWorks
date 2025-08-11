/** RFC 6902 JSON Patch operation. */
export interface JsonPatchOperation {
  readonly op: 'add' | 'remove' | 'replace' | 'move' | 'copy' | 'test';
  readonly path: string;
  readonly value?: unknown;
  readonly from?: string;
}
