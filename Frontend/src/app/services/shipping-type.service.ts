import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateShippingTypeRequest,
  UpdateShippingTypeRequest,
  ShippingTypeResponse
} from '../models/shipping-type.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ShippingTypeService {
  private apiUrl = `${environment.apiBaseUrl}/api/shipping-types`;

  constructor(private http: HttpClient) {}

  create(request: CreateShippingTypeRequest): Observable<ShippingTypeResponse> {
    return this.http.post<ShippingTypeResponse>(this.apiUrl, request);
  }

  update(id: string, request: UpdateShippingTypeRequest): Observable<ShippingTypeResponse> {
    return this.http.put<ShippingTypeResponse>(`${this.apiUrl}/${id}`, request);
  }

  getAll(): Observable<ShippingTypeResponse[]> {
    return this.http.get<ShippingTypeResponse[]>(this.apiUrl);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
