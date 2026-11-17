export type ConfigurationGroup<T> = {
  type: 'cell' | 'table';
  uid: string;
  value: T;
};
