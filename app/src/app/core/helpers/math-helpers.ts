import { isNumber } from 'lodash';

export class MathHelpers {
  public static round(value: number, dp = 3): number {
    return Math.round(value * Math.pow(10, dp)) / Math.pow(10, dp);
  }

  public static checkNan(value: number): number | undefined {
    return isNumber(value) && isNaN(value) ? undefined : value;
  }

  public static isNan(value: number): boolean {
    return isNumber(value) && isNaN(value);
  }
}
