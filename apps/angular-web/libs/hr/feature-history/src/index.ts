export { DepartmentTimelineComponent } from './lib/department-timeline/department-timeline';
export { PayHistoryTableComponent } from './lib/pay-history-table/pay-history-table';
export { CombinedTimelineComponent } from './lib/combined-timeline/combined-timeline';
export { buildCombinedTimeline, computePayDeltas } from './lib/combined-timeline/build-combined-timeline';
export type {
  CombinedTimelineEvent,
  CombinedTimelineGroup,
  DepartmentTimelineEvent,
  PayTimelineEvent,
  PayDelta,
} from './lib/combined-timeline/build-combined-timeline';
