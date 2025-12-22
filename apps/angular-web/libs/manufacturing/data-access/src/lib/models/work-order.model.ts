/** Production.WorkOrder list-row projection returned by GET /v1/work-orders. */
export interface WorkOrder {
  readonly workOrderId: number;
  readonly productId: number;
  readonly productName: string;
  readonly orderedQty: number;
  readonly stockedQty: number;
  readonly scrappedQty: number;
  readonly yieldRate: number;
  readonly startDate: string;
  readonly endDate: string | null;
  readonly dueDate: string;
  readonly isCompletedLate: boolean;
}
