import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { OrderService } from '../../services/order.service';
import {
  OrderSummary,
  SearchCustomerOrdersRequest
} from '../../models/order.model';

@Component({
  selector: 'app-my-orders',
  standalone: false,
  templateUrl: './my-orders.component.html',
  styleUrl: './my-orders.component.css'
})
export class MyOrdersComponent implements OnInit, OnDestroy {

  orders: OrderSummary[] = [];

  request: SearchCustomerOrdersRequest = {
    customerId: '',
  status: ''
  };

  errorMessage: string | null = null;

  private searchSubscription: Subscription | null = null;

  constructor(private orderService: OrderService) {}

  ngOnInit(): void {
    const userJson = localStorage.getItem('user');

    if (!userJson) {
      this.errorMessage = 'Usuario no encontrado';
      return;
    }

    const user = JSON.parse(userJson);
    this.request.customerId = user.id;

    this.search();
  }

  ngOnDestroy(): void {
    this.searchSubscription?.unsubscribe();
  }

statuses = [
  { value: 'Pending', label: 'Pendiente' },
  { value: 'Prepared', label: 'Preparado' },
  { value: 'InTransit', label: 'En camino' },
  { value: 'Delayed', label: 'Demorado' },
  { value: 'Delivered', label: 'Entregado' },
  { value: 'Cancelled', label: 'Cancelado' },
  { value: 'NotDelivered', label: 'No entregado' }
];

getStatusName(status: string | number): string {
  const map: { [key: string]: string } = {
    '0': 'Pendiente',
    '1': 'Preparado',
    '2': 'Cancelado',
    '3': 'En camino',
    '4': 'Entregado',
    '5': 'No entregado',
    '6': 'Demorado',
    Pending: 'Pendiente',
    Prepared: 'Preparado',
    Cancelled: 'Cancelado',
    InTransit: 'En camino',
    Delivered: 'Entregado',
    NotDelivered: 'No entregado',
    Delayed: 'Demorado'
  };

  return map[status.toString()] || status.toString();
}

  search(): void {
  this.errorMessage = null;

  if (this.request.dateFrom && this.request.dateTo && this.request.dateFrom > this.request.dateTo) {
    this.errorMessage = 'La fecha desde no puede ser mayor que la fecha hasta';
    this.orders = [];
    return;
  }

  this.searchSubscription = this.orderService.getCustomerOrders(this.request).subscribe({
    next: orders => {
  this.orders = orders;

  if (orders.length === 0) {
    this.errorMessage = 'No hay pedidos que cumplan con los filtros';
  } else {
    this.errorMessage = null;
  }
},
    error: err => {
      const error = err.error;

      this.errorMessage =
        typeof error === 'string'
          ? error
          : error?.message || 'Error al obtener pedidos';
    }
  });
}
}