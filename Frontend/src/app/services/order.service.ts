import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateOrderRequest,
  CreateOrderResponse,
  OrderAction,
  OrderDetail,
  OrderStatusChangeResponse,
  OrderSummary,
  SearchCustomerOrdersRequest,
  SearchFilteredOrdersRequest
} from '../models/order.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private apiUrl = `${environment.apiBaseUrl}/api/orders`;

  constructor(private http: HttpClient) {}

  create(request: CreateOrderRequest): Observable<CreateOrderResponse> {
    return this.http.post<CreateOrderResponse>(this.apiUrl, request);
  }

  getById(id: string): Observable<OrderDetail> {
    return this.http.get<OrderDetail>(`${this.apiUrl}/${id}`);
  }

  // PATCH api/orders/{id} con body { action: OrderAction }
  executeAction(id: string, action: OrderAction): Observable<OrderStatusChangeResponse> {
    return this.http.patch<OrderStatusChangeResponse>(
      `${this.apiUrl}/${id}`,
      { action }
    );
  }

  getCustomerOrders(request: SearchCustomerOrdersRequest): Observable<OrderSummary[]> {
  let params = new HttpParams().set('customerId', request.customerId);

  if (request.status) params = params.set('status', request.status);
  if (request.dateFrom) params = params.set('from', `${request.dateFrom}T00:00:00`);
  if (request.dateTo) params = params.set('to', `${request.dateTo}T23:59:59`);
  if (request.page) params = params.set('page', request.page.toString());
  if (request.pageSize) params = params.set('pageSize', request.pageSize.toString());

  return this.http.get<OrderSummary[]>(`${this.apiUrl}/customer`, { params });
}

  getFilteredOrders(request: SearchFilteredOrdersRequest): Observable<OrderSummary[]> {

  let params = new HttpParams()
    .set('from', `${request.dateFrom}T00:00:00`)
    .set('to', `${request.dateTo}T23:59:59`);

  if (request.status) {
    params = params.set('status', request.status);
  }

  if (request.address) {
    params = params.set('addressText', request.address);
  }

  if (request.page) {
    params = params.set('page', request.page.toString());
  }

  if (request.pageSize) {
    params = params.set('pageSize', request.pageSize.toString());
  }

  return this.http.get<OrderSummary[]>(
    `${this.apiUrl}/customerF`,
    { params }
  );
}
}
