/** Quality scorecard response returned by GET /v1/manufacturing/quality-scorecard. */
export interface ManufacturingQualityScorecard {
  readonly top5ByScrapped: QualityScorecardProduct[];
  readonly bottom5ByYield: QualityScorecardProduct[];
  readonly scrapReasonBreakdown: QualityScorecardScrapReason[];
}

/** Aggregate quality metrics for a product ranking row. */
export interface QualityScorecardProduct {
  readonly productId: number;
  readonly productName: string;
  readonly orderedQty: number;
  readonly stockedQty: number;
  readonly scrappedQty: number;
  readonly yieldPct: number;
  readonly scrapPct: number;
}

/** Aggregate scrap metrics for a scrap-reason breakdown row. */
export interface QualityScorecardScrapReason {
  readonly scrapReasonId: number;
  readonly scrapReasonName: string;
  readonly scrappedQty: number;
  readonly scrapPct: number;
}
