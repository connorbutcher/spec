/** The live connection to the server: `connected` is the only state in which changes arrive as they happen. */
export type SheetConnectionState = 'connecting' | 'connected' | 'reconnecting' | 'disconnected';
