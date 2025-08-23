import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ENVIRONMENT } from '../environment/environment.token';
import { Environment } from '../environment/environment.model';
import { AppInsightsErrorHandler, AppInsightsService } from './app-insights.service';

const mockTrackEvent = vi.fn();
const mockTrackException = vi.fn();
const mockLoadAppInsights = vi.fn();
const mockAddTelemetryInitializer = vi.fn();

vi.mock('@microsoft/applicationinsights-web', () => ({
  ApplicationInsights: vi.fn().mockImplementation(function (this: object) {
    return {
      loadAppInsights: mockLoadAppInsights,
      addTelemetryInitializer: mockAddTelemetryInitializer,
      trackEvent: mockTrackEvent,
      trackException: mockTrackException,
    };
  }),
}));

vi.mock('@microsoft/applicationinsights-angularplugin-js', () => ({
  AngularPlugin: vi.fn().mockImplementation(function (this: object) {
    return { identifier: 'AngularPlugin' };
  }),
}));

const envWithAppInsights: Environment = {
  production: false,
  defaultLocale: 'en',
  api: { primary: { baseUrl: 'https://localhost:5001/api', name: 'Primary' } },
  appInsights: {
    connectionString:
      'InstrumentationKey=test-key;IngestionEndpoint=https://westus3-1.in.applicationinsights.azure.com/',
    cloudRoleName: 'AdventureWorks.Web',
  },
};

const envWithoutAppInsights: Environment = {
  production: false,
  defaultLocale: 'en',
  api: { primary: { baseUrl: 'https://localhost:5001/api', name: 'Primary' } },
};

const envWithPlaceholder: Environment = {
  production: false,
  defaultLocale: 'en',
  api: { primary: { baseUrl: 'https://localhost:5001/api', name: 'Primary' } },
  appInsights: {
    connectionString: '__APP_INSIGHTS_CONNECTION_STRING__',
    cloudRoleName: 'AdventureWorks.Web',
  },
};

function setup(env: Environment): AppInsightsService {
  TestBed.configureTestingModule({
    providers: [{ provide: ENVIRONMENT, useValue: env }, provideRouter([])],
  });
  return TestBed.inject(AppInsightsService);
}

describe('AppInsightsService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('initialize()', () => {
    it('does not load SDK when connection string is absent', async () => {
      const service = setup(envWithoutAppInsights);
      await service.initialize();
      expect(mockLoadAppInsights).not.toHaveBeenCalled();
    });

    it('does not load SDK when connection string is a placeholder', async () => {
      const service = setup(envWithPlaceholder);
      await service.initialize();
      expect(mockLoadAppInsights).not.toHaveBeenCalled();
    });

    it('loads the SDK when a valid connection string is provided', async () => {
      const service = setup(envWithAppInsights);
      await service.initialize();
      expect(mockLoadAppInsights).toHaveBeenCalledOnce();
    });
  });

  describe('trackEvent()', () => {
    it('queues the event when called before initialize()', async () => {
      const service = setup(envWithAppInsights);
      service.trackEvent('PageView', { page: '/home' });
      expect(mockTrackEvent).not.toHaveBeenCalled();

      await service.initialize();
      expect(mockTrackEvent).toHaveBeenCalledWith({ name: 'PageView' }, { page: '/home' });
    });

    it('forwards the event immediately when called after initialize()', async () => {
      const service = setup(envWithAppInsights);
      await service.initialize();
      service.trackEvent('ButtonClick');
      expect(mockTrackEvent).toHaveBeenCalledWith({ name: 'ButtonClick' }, undefined);
    });

    it('is a no-op when connection string is absent', async () => {
      const service = setup(envWithoutAppInsights);
      await service.initialize();
      service.trackEvent('PageView');
      expect(mockTrackEvent).not.toHaveBeenCalled();
    });
  });

  describe('trackException()', () => {
    it('queues the exception when called before initialize()', async () => {
      const service = setup(envWithAppInsights);
      const error = new Error('boom');
      service.trackException(error);
      expect(mockTrackException).not.toHaveBeenCalled();

      await service.initialize();
      expect(mockTrackException).toHaveBeenCalledWith({ exception: error });
    });

    it('forwards the exception immediately when called after initialize()', async () => {
      const service = setup(envWithAppInsights);
      await service.initialize();
      const error = new Error('post-init error');
      service.trackException(error);
      expect(mockTrackException).toHaveBeenCalledWith({ exception: error });
    });

    it('is a no-op when connection string is absent', async () => {
      const service = setup(envWithoutAppInsights);
      await service.initialize();
      service.trackException(new Error('ignored'));
      expect(mockTrackException).not.toHaveBeenCalled();
    });
  });

  describe('queue flushing', () => {
    it('flushes multiple queued events in order', async () => {
      const service = setup(envWithAppInsights);
      const error = new Error('queued error');
      service.trackEvent('First');
      service.trackException(error);
      service.trackEvent('Second');

      await service.initialize();

      expect(mockTrackEvent).toHaveBeenCalledTimes(2);
      expect(mockTrackEvent).toHaveBeenNthCalledWith(1, { name: 'First' }, undefined);
      expect(mockTrackEvent).toHaveBeenNthCalledWith(2, { name: 'Second' }, undefined);
      expect(mockTrackException).toHaveBeenCalledWith({ exception: error });
    });

    it('is idempotent — subsequent calls are no-ops', async () => {
      const service = setup(envWithAppInsights);
      service.trackEvent('Once');
      await service.initialize();
      expect(mockLoadAppInsights).toHaveBeenCalledTimes(1);
      expect(mockTrackEvent).toHaveBeenCalledTimes(1);

      await service.initialize();
      expect(mockLoadAppInsights).toHaveBeenCalledTimes(1);
      expect(mockTrackEvent).toHaveBeenCalledTimes(1);
    });
  });
});

describe('AppInsightsErrorHandler', () => {
  let handler: AppInsightsErrorHandler;
  let trackException: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    trackException = vi.fn();
    TestBed.configureTestingModule({
      providers: [
        AppInsightsErrorHandler,
        { provide: AppInsightsService, useValue: { trackException } },
        { provide: ENVIRONMENT, useValue: envWithoutAppInsights },
        provideRouter([]),
      ],
    });
    handler = TestBed.inject(AppInsightsErrorHandler);
    vi.spyOn(console, 'error').mockImplementation(() => undefined);
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('passes Error instances directly to trackException', () => {
    const error = new Error('test error');
    handler.handleError(error);
    expect(trackException).toHaveBeenCalledWith(error);
  });

  it('wraps non-Error values in a new Error', () => {
    handler.handleError('string error');
    expect(trackException).toHaveBeenCalledWith(new Error('string error'));
  });

  it('logs the original value via console.error', () => {
    const error = new Error('test error');
    handler.handleError(error);
    expect(console.error).toHaveBeenCalledWith(error);
  });
});
