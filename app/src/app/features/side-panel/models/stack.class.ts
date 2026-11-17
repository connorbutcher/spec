export class Stack<T> {
  private _storage: T[] = [];

  constructor(private capacity = Infinity) {}

  push(item: T): void {
    if (this.size === this.capacity) {
      throw Error('Stack has reached max capacity, you cannot add more items');
    }

    this._storage.push(item);
  }

  pop(): T | undefined {
    return this._storage.pop();
  }

  peek(): T | undefined {
    return this._storage[this.size - 1];
  }

  public getAll(): T[] {
    return this._storage;
  }

  public get size(): number {
    return this._storage.length;
  }
}
