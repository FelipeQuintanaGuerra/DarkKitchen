import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreatePromotionRequest,
  UpdatePromotionRequest,
  SearchPromotionRequest,
  PromotionSummary
} from '../models/promotion.model';
import { CommercialLine } from '../models/commercial-line.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PromotionService {
  private apiUrl = `${environment.apiBaseUrl}/api/promotions`;

  constructor(private http: HttpClient) {}

  getActive(request: SearchPromotionRequest): Observable<PromotionSummary[]> {
    let params = new HttpParams();
    if (request.dateFrom) params = params.set('dateFrom', request.dateFrom);
    if (request.dateTo) params = params.set('dateTo', request.dateTo);
    if (request.commercialLineId) params = params.set('commercialLineId', request.commercialLineId);
    if (request.productId) params = params.set('productId', request.productId);
    if (request.page) params = params.set('page', request.page.toString());
    if (request.pageSize) params = params.set('pageSize', request.pageSize.toString());
    return this.http.get<PromotionSummary[]>(this.apiUrl, { params });
  }

  create(request: CreatePromotionRequest): Observable<PromotionSummary> {
    return this.http.post<PromotionSummary>(this.apiUrl, request);
  }

  getCommercialLines(): Observable<CommercialLine[]> {
    return this.http.get<CommercialLine[]>(`${this.apiUrl}/commercial-lines`);
  }

  update(id: string, request: UpdatePromotionRequest): Observable<PromotionSummary> {
    return this.http.put<PromotionSummary>(`${this.apiUrl}/${id}`, request);
  }

  addProduct(promotionId: string, productId: string): Observable<void> {
  return this.http.post<void>(
    `${this.apiUrl}/${promotionId}/products`,
    { productId }
  );
}
  removeProduct(promotionId: string, productId: string): Observable<void> {
  const params = new HttpParams().set('productId', productId);

  return this.http.delete<void>(
    `${this.apiUrl}/${promotionId}/products`,
    { params }
  );
}
}
