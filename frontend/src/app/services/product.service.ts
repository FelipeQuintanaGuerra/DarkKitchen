import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ProductSummary, SearchProductRequest } from '../models/product.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private apiUrl = `${environment.apiBaseUrl}/api/products`;

  constructor(private http: HttpClient) {}

  create(formData: FormData): Observable<ProductSummary> {
    return this.http.post<ProductSummary>(this.apiUrl, formData);
  }

  update(id: string, formData: FormData): Observable<ProductSummary> {
    return this.http.put<ProductSummary>(`${this.apiUrl}/${id}`, formData);
  }

  search(request: SearchProductRequest): Observable<ProductSummary[]> {
    let params = new HttpParams();
    if (request.name) params = params.set('name', request.name);
    if (request.commercialLineId) params = params.set('commercialLineId', request.commercialLineId);
    if (request.categoryId) params = params.set('categoryId', request.categoryId);
    if (request.page) params = params.set('page', request.page.toString());
    if (request.pageSize) params = params.set('pageSize', request.pageSize.toString());
    return this.http.get<ProductSummary[]>(this.apiUrl, { params });
  }

  changeActivation(id: string): Observable<ProductSummary> {
    return this.http.patch<ProductSummary>(`${this.apiUrl}/${id}`, {});
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getById(id: string): Observable<ProductSummary> {
    return this.http.get<ProductSummary>(`${this.apiUrl}/${id}`);
  }
}
