export type SignalRConnectionStatus = 'disconnected' | 'connecting' | 'connected' | 'reconnecting';

export type SignalRHandler<T> = (payload: T) => void;
