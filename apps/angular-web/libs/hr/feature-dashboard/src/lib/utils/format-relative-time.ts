const MILLISECONDS_PER_SECOND = 1000;
const SECONDS_PER_MINUTE = 60;
const MINUTES_PER_HOUR = 60;
const HOURS_PER_DAY = 24;
const relativeTimeFormat = new Intl.RelativeTimeFormat('en-US', { numeric: 'auto' });

/**
 * Formats `date` relative to `now` as a short, human-readable string (e.g. "2 minutes ago",
 * "just now", "1 hour ago"). Used for the HR dashboard's "Last updated" label (US-767).
 *
 * Each larger unit is derived by rounding the smaller unit already computed (seconds -> minutes ->
 * hours -> days), rather than rounding `diffMs` independently per bucket. Rounding independently
 * per bucket previously let a diff like 59,900ms select the "second" bucket (59,900 < 60,000ms)
 * but then round to display "60 seconds ago" instead of "1 minute ago" — the boundary and the
 * displayed value must be computed from the same rounded number.
 */
export function formatRelativeTime(date: Date, now: Date = new Date()): string {
  const diffMs = now.getTime() - date.getTime();

  if (diffMs < MILLISECONDS_PER_SECOND) {
    return 'just now';
  }

  const diffSeconds = Math.round(diffMs / MILLISECONDS_PER_SECOND);
  if (diffSeconds < SECONDS_PER_MINUTE) {
    return relativeTimeFormat.format(-diffSeconds, 'second');
  }

  const diffMinutes = Math.round(diffSeconds / SECONDS_PER_MINUTE);
  if (diffMinutes < MINUTES_PER_HOUR) {
    return relativeTimeFormat.format(-diffMinutes, 'minute');
  }

  const diffHours = Math.round(diffMinutes / MINUTES_PER_HOUR);
  if (diffHours < HOURS_PER_DAY) {
    return relativeTimeFormat.format(-diffHours, 'hour');
  }

  const diffDays = Math.round(diffHours / HOURS_PER_DAY);
  return relativeTimeFormat.format(-diffDays, 'day');
}
