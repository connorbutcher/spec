import { ConnectedUser } from './connected-user';

export type CheckoutRequest = {
  requestContextId: string;
  id: number;
  requestingUser: ConnectedUser;
  respondingUser: ConnectedUser;
  requestedDateTime: string;
};
