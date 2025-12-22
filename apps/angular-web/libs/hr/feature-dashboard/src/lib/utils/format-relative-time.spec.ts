import { formatRelativeTime } from './format-relative-time';

describe('formatRelativeTime', () => {
  const now = new Date('2026-07-26T12:00:00.000Z');

  it('returns "just now" for sub-second differences', () => {
    const date = new Date(now.getTime() - 500);
    expect(formatRelativeTime(date, now)).toBe('just now');
  });

  it('formats seconds ago', () => {
    const date = new Date(now.getTime() - 45_000);
    expect(formatRelativeTime(date, now)).toBe('45 seconds ago');
  });

  it('formats minutes ago', () => {
    const date = new Date(now.getTime() - 2 * 60_000);
    expect(formatRelativeTime(date, now)).toBe('2 minutes ago');
  });

  it('formats a single minute ago in singular form', () => {
    const date = new Date(now.getTime() - 60_000);
    expect(formatRelativeTime(date, now)).toBe('1 minute ago');
  });

  it('formats hours ago', () => {
    const date = new Date(now.getTime() - 3 * 60 * 60_000);
    expect(formatRelativeTime(date, now)).toBe('3 hours ago');
  });

  it('formats days ago', () => {
    const date = new Date(now.getTime() - 2 * 24 * 60 * 60_000);
    expect(formatRelativeTime(date, now)).toBe('2 days ago');
  });

  it('defaults "now" to the current time when omitted', () => {
    const date = new Date();
    expect(formatRelativeTime(date)).toBe('just now');
  });

  it('rounds a near-minute boundary up to "1 minute ago" instead of "60 seconds ago"', () => {
    // 59,900ms is < 60,000ms so it's still in the "seconds" bucket by raw diff, but rounds to 60 —
    // the display and the bucket decision must agree, not independently round the same diff twice.
    const date = new Date(now.getTime() - 59_900);
    expect(formatRelativeTime(date, now)).toBe('1 minute ago');
  });

  it('rounds a near-hour boundary up to "1 hour ago" instead of "60 minutes ago"', () => {
    const date = new Date(now.getTime() - (60 * 60_000 - 100));
    expect(formatRelativeTime(date, now)).toBe('1 hour ago');
  });

  it('rounds a near-day boundary up to "yesterday" instead of "24 hours ago" (numeric: "auto" special-cases 1 day)', () => {
    const date = new Date(now.getTime() - (24 * 60 * 60_000 - 1000));
    expect(formatRelativeTime(date, now)).toBe('yesterday');
  });
});
