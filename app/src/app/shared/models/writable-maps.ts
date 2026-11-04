/** `T` with each read-only map it holds as a `Map`, for the code that fills them. */
export type WritableMaps<T> = {
  [K in keyof T]: T[K] extends ReadonlyMap<infer Key, infer Value> ? Map<Key, Value> : T[K];
};
