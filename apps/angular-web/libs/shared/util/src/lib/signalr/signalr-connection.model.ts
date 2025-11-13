export type SignalRConnectionStatus = 'disconnected' | 'connecting' | 'connected' | 'reconnecting';

export type SignalRHandler<T> = (payload: T) => void;

/** Matches the shape of `EntityChangedNotification` serialized over SignalR from `SignalRBroadcastHandler`. */
export interface EntityChangedEvent {
  entityType: string;
  entityId: number;
  action: string;
  userName: string;
  timestamp: string; // ISO 8601 — .NET DateTime serializes as string over SignalR JSON
}

/** Reserved for a future backend notification type; no server producer exists yet. */
export interface DashboardRefreshEvent {
  triggeredBy: string;
  timestamp: string; // ISO 8601 — .NET DateTime serializes as string over SignalR JSON
}
