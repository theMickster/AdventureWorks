if (typeof window.matchMedia !== 'function') {
  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    value: (query: string) => ({
      matches: false,
      media: query,
      onchange: null,
      addListener: () => undefined,
      removeListener: () => undefined,
      addEventListener: () => undefined,
      removeEventListener: () => undefined,
      dispatchEvent: () => false,
    }),
  });
}

if (typeof window.ResizeObserver !== 'function') {
  Object.defineProperty(window, 'ResizeObserver', {
    writable: true,
    value: class ResizeObserverStub {
      observe(): void {
        return undefined;
      }
      unobserve(): void {
        return undefined;
      }
      disconnect(): void {
        return undefined;
      }
    },
  });
}

const originalGetContext = HTMLCanvasElement.prototype.getContext as (...args: unknown[]) => unknown;
HTMLCanvasElement.prototype.getContext = function (this: HTMLCanvasElement, contextId: string, ...args: unknown[]) {
  if (contextId === '2d') {
    return new Proxy({ canvas: this }, { get: (target, prop) => target[prop as keyof typeof target] ?? (() => undefined) });
  }
  return originalGetContext.apply(this, [contextId, ...args]);
} as typeof HTMLCanvasElement.prototype.getContext;

if (typeof Element.prototype.scrollIntoView !== 'function') {
  Element.prototype.scrollIntoView = function (): void {
    return undefined;
  };
}

if (typeof localStorage === 'undefined') {
  throw new Error('localStorage is unexpectedly unavailable in the jsdom test environment.');
}
