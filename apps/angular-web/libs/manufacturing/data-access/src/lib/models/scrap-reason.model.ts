/** A scrap reason from GET /v1/scrap-reasons, used to populate the work order list's scrap reason filter. */
export interface ScrapReason {
  readonly scrapReasonId: number;
  readonly name: string;
}
