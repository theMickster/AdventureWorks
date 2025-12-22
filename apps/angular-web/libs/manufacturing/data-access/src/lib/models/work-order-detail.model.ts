/** Production.WorkOrder full detail projection returned by GET /v1/work-orders/:id. */
export interface WorkOrderDetail {
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
  readonly daysLate: number | null;
  readonly scrapReasonId: number | null;
  readonly scrapReasonName: string | null;
}
