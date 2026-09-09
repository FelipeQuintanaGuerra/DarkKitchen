import { Pipe, PipeTransform } from '@angular/core';
import { environment } from '../../environments/environment';

@Pipe({
  name: 'absoluteUrl',
  standalone: false
})
export class AbsoluteUrlPipe implements PipeTransform {
  transform(url: string | null | undefined): string {
    if (!url) return '';

    // Si ya es una URL absoluta, devuélvela tal cual
    if (url.startsWith('http://') || url.startsWith('https://')) {
      return url;
    }

    // Si es relativa, prefija con la URL base del API
    return `${environment.apiBaseUrl}${url}`;
  }
}

