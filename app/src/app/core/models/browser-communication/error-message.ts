import { BrowserMessage } from './browser-message';

export type ErrorPayload = {
  code: string;
  details?: unknown;
};
export type ErrorMessage = BrowserMessage<ErrorPayload>;
