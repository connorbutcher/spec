import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'avatar',
  standalone: true,
})
export class AvatarPipe implements PipeTransform {
  transform(value: string, ...args: unknown[]): string {
    if (!value) {
      return '';
    }

    return value
      .split(/[ .]+/)
      .map((n) => n[0])
      .join('')
      .replace('.', '')
      .toLocaleUpperCase();
  }
}
