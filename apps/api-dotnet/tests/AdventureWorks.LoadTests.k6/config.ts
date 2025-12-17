const defaultThresholds = Object.freeze({
  checks: ['rate>0.95'],
  http_req_duration: ['p(95)<1500'],
  http_req_failed: ['rate<0.05'],
});

const profileConfigurations = Object.freeze({
  smoke: {
    options: {
      vus: 1,
      duration: '30s',
    },
  },
  load: {
    options: {
      stages: [
        { duration: '2m', target: 50 },
        { duration: '5m', target: 50 },
        { duration: '1m', target: 0 },
      ],
    },
    thresholds: {
      checks: ['rate>0.99'],
      http_req_duration: ['p(95)<500', 'p(99)<1500'],
      http_req_failed: ['rate<0.01'],
    },
  },
  stress: {
    options: {
      stages: [
        { duration: '2m', target: 20 },
        { duration: '3m', target: 60 },
        { duration: '4m', target: 100 },
        { duration: '3m', target: 125 },
        { duration: '2m', target: 0 },
      ],
    },
    thresholds: {
      checks: ['rate>0.85'],
      http_req_duration: ['p(95)<3000'],
      http_req_failed: ['rate<0.20'],
      breaking_point_error_rate: ['rate<0.30'],
    },
  },
});

export const loadTestConfig = Object.freeze({
  baseUrl: __ENV.BASE_URL || 'https://localhost:44369',
  authToken: __ENV.K6_AUTH_TOKEN || __ENV.AUTH_TOKEN || '',
  insecureSkipTLSVerify: (__ENV.K6_INSECURE_SKIP_TLS_VERIFY || 'true').toLowerCase() === 'true',
});

type SummaryData = {
  metrics?: Record<string, {
    type?: string;
    contains?: string;
    values?: Record<string, number>;
    thresholds?: Record<string, { ok: boolean }>;
  }>;
};

function mergeThresholds(baseThresholds: Record<string, string[]>, profileThresholds: Record<string, string[]>, thresholdOverrides: Record<string, string[]>) {
  return {
    ...baseThresholds,
    ...profileThresholds,
    ...thresholdOverrides,
  };
}

function escapeHtml(value: string): string {
  return value
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}

function metricValue(values: Record<string, number> | undefined, ...keys: string[]): string {
  if (!values) {
    return '-';
  }

  for (const key of keys) {
    if (values[key] !== undefined) {
      const value = values[key];
      if (key.startsWith('p(') || key === 'avg' || key === 'med' || key === 'min' || key === 'max') {
        return `${value.toFixed(2)} ms`;
      }

      if (key === 'rate') {
        return `${(value * 100).toFixed(2)}%`;
      }

      return value.toFixed(2);
    }
  }

  return '-';
}

function thresholdStatus(thresholds: Record<string, { ok: boolean }> | undefined): string {
  if (!thresholds || Object.keys(thresholds).length === 0) {
    return 'n/a';
  }

  return Object.values(thresholds).every((t) => t.ok) ? 'PASS' : 'FAIL';
}

function buildHtmlSummary(profileName: string, data: SummaryData): string {
  const metrics = data.metrics || {};
  const rows = Object.entries(metrics)
    .map(([name, metric]) => {
      const values = metric.values;
      return `<tr>
        <td>${escapeHtml(name)}</td>
        <td>${escapeHtml(metric.type || '-')}</td>
        <td>${escapeHtml(metricValue(values, 'avg', 'value'))}</td>
        <td>${escapeHtml(metricValue(values, 'p(95)', 'p(90)'))}</td>
        <td>${escapeHtml(metricValue(values, 'p(99)', 'max'))}</td>
        <td>${escapeHtml(metricValue(values, 'rate'))}</td>
        <td>${escapeHtml(thresholdStatus(metric.thresholds))}</td>
      </tr>`;
    })
    .join('\n');

  return `<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>k6 Summary - ${escapeHtml(profileName)}</title>
  <style>
    body { font-family: -apple-system, BlinkMacSystemFont, Segoe UI, sans-serif; margin: 24px; color: #111; }
    h1 { margin: 0 0 12px 0; font-size: 24px; }
    .meta { margin-bottom: 16px; color: #444; }
    table { border-collapse: collapse; width: 100%; }
    th, td { border: 1px solid #ddd; padding: 8px; text-align: left; font-size: 13px; }
    th { background: #f5f5f5; }
  </style>
</head>
<body>
  <h1>k6 Summary: ${escapeHtml(profileName)}</h1>
  <div class="meta">Generated: ${escapeHtml(new Date().toISOString())}</div>
  <table>
    <thead>
      <tr>
        <th>Metric</th>
        <th>Type</th>
        <th>Avg/Value</th>
        <th>P95/P90</th>
        <th>P99/Max</th>
        <th>Rate</th>
        <th>Thresholds</th>
      </tr>
    </thead>
    <tbody>
      ${rows}
    </tbody>
  </table>
</body>
</html>`;
}

function buildTextSummary(profileName: string, data: SummaryData): string {
  const metrics = data.metrics || {};
  const interestingMetrics = ['checks', 'http_req_duration', 'http_req_failed', 'http_reqs', 'vus_max', 'iterations'];

  const lines = [`k6 summary (${profileName})`, ''];

  for (const metricName of interestingMetrics) {
    const metric = metrics[metricName];
    if (!metric) {
      continue;
    }

    lines.push(
      `${metricName}: avg/value=${metricValue(metric.values, 'avg', 'value')} p95/p90=${metricValue(metric.values, 'p(95)', 'p(90)')} threshold=${thresholdStatus(metric.thresholds)}`,
    );
  }

  return `${lines.join('\n')}\n`;
}

export function getProfileOptions(profileName: 'smoke' | 'load' | 'stress', optionOverrides: Record<string, unknown> = {}, thresholdOverrides: Record<string, string[]> = {}) {
  const profileConfiguration = profileConfigurations[profileName];

  return {
    ...profileConfiguration.options,
    ...optionOverrides,
    insecureSkipTLSVerify: loadTestConfig.insecureSkipTLSVerify,
    thresholds: mergeThresholds(
      defaultThresholds,
      profileConfiguration.thresholds || {},
      thresholdOverrides,
    ),
  };
}

export function createHandleSummary(profileName: string) {
  return (data: SummaryData) => ({
    [`results/${profileName}-summary.html`]: buildHtmlSummary(profileName, data),
    [`results/${profileName}-summary.json`]: JSON.stringify(data, null, 2),
    stdout: buildTextSummary(profileName, data),
  });
}
