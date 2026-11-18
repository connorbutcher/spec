import { PropertyValue } from './property-value';

export type StringPropertyValue = PropertyValue & {
  value?: string;
};
