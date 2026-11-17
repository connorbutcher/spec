export type BrowserMessage<T> = {
  type: 'initialize' | 'dataset' | 'complete';
  status: 'success' | 'error' | 'pending';
  message?: string;
  payload: T;
};
