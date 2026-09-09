import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpResponse
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';

@Injectable()
export class ImageInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      map(event => {
        if (event instanceof HttpResponse && event.body) {
          this.processImageUrls(event.body);
        }
        return event;
      })
    );
  }

  private processImageUrls(obj: any): void {
    if (!obj || typeof obj !== 'object') return;

    // Si es un array, procesa cada elemento
    if (Array.isArray(obj)) {
      obj.forEach(item => this.processImageUrls(item));
      return;
    }

    // Si tiene imageUrls, convierte URLs relativas a absolutas
    if (obj.imageUrls && Array.isArray(obj.imageUrls)) {
      obj.imageUrls = obj.imageUrls.map((url: string) => {
        if (url && !url.startsWith('http')) {
          return `${environment.apiBaseUrl}${url}`;
        }
        return url;
      });
    }

    // Recursivamente procesa propiedades anidadas
    Object.values(obj).forEach(value => this.processImageUrls(value));
  }
}

