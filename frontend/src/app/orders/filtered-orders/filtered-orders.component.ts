import { Component, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { OrderService } from '../../services/order.service';
import {
  OrderSummary,
  SearchFilteredOrdersRequest,
  OrderDetail,
  OrderAction
} from '../../models/order.model';

@Component({
  selector: 'app-filtered-orders',
  standalone: false,
  templateUrl: './filtered-orders.component.html',
  styleUrl: './filtered-orders.component.css'
})
export class FilteredOrdersComponent implements OnDestroy {
  orders: OrderSummary[] = [];

  selectedOrder: OrderDetail | null = null;

  request: SearchFilteredOrdersRequest = {
    dateFrom: '',
    dateTo: '',
    status: '',
    address: ''
  };

  statuses = [
    { value: 'Pending', label: 'Pendiente' },
    { value: 'Prepared', label: 'Preparado' },
    { value: 'InTransit', label: 'En camino' },
    { value: 'Delayed', label: 'Demorado' },
    { value: 'Delivered', label: 'Entregado' },
    { value: 'Cancelled', label: 'Cancelado' },
    { value: 'NotDelivered', label: 'No entregado' }
  ];

  errorMessage: string | null = null;

  private searchSubscription: Subscription | null = null;
  private detailSubscription: Subscription | null = null;

  private roleMap: { [key: number]: string } = {
    0: 'Client',
    1: 'Dispatcher',
    2: 'Admin'
  };

  constructor(private orderService: OrderService) {}

  ngOnDestroy(): void {
    this.searchSubscription?.unsubscribe();
    this.detailSubscription?.unsubscribe();
  }

  get userRole(): string {
    const userJson = localStorage.getItem('user');

    if (!userJson) {
      return '';
    }

    const user = JSON.parse(userJson);
    const role = user.role;

    return typeof role === 'number'
      ? this.roleMap[role] || ''
      : role || '';
  }

  isAdmin(): boolean {
    return this.userRole === 'Admin';
  }

  isDispatcher(): boolean {
    return this.userRole === 'Dispatcher';
  }

  search(): void {
    this.errorMessage = null;
    this.orders = [];
    this.selectedOrder = null;

    if (!this.request.dateFrom || !this.request.dateTo) {
      this.errorMessage = 'Las fechas desde y hasta son obligatorias';
      return;
    }

    if (this.request.dateFrom > this.request.dateTo) {
      this.errorMessage = 'La fecha desde no puede ser mayor que la fecha hasta';
      return;
    }

    this.searchSubscription = this.orderService.getFilteredOrders(this.request).subscribe({
      next: orders => {
        this.orders = orders;

        if (orders.length === 0) {
          this.errorMessage = 'No hay pedidos que cumplan con los filtros';
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

  loadDetail(orderNumber: string): void {
    this.errorMessage = null;

    this.detailSubscription = this.orderService.getById(orderNumber).subscribe({
      next: order => {
        this.selectedOrder = order;
      },
      error: err => {
        const error = err.error;
        this.errorMessage =
          typeof error === 'string'
            ? error
            : error?.message || 'Error al obtener detalle';
      }
    });
  }

  executeAction(
    order: OrderSummary,
    action: OrderAction
  ): void {

    this.errorMessage = null;

    this.orderService.executeAction(
      order.orderNumber,
      action
    ).subscribe({
      next: response => {

        order.status = response.status;

        if (
          this.selectedOrder &&
          this.selectedOrder.orderNumber === order.orderNumber
        ) {
          this.loadDetail(order.orderNumber);
        }
      },
      error: err => {
        const error = err.error;

        this.errorMessage =
          typeof error === 'string'
            ? error
            : error?.message || 'Error al cambiar estado';
      }
    });
  }

  canPrepare(status: string | number): boolean {
  return (this.isDispatcher() || this.isAdmin()) &&
    ['Pending', 'Delayed', '0', '6'].includes(status.toString());
}

canCancel(status: string | number): boolean {
  return this.isAdmin() &&
    ['Pending', 'Delayed', '0', '6'].includes(status.toString());
}

canDelay(status: string | number): boolean {
  return this.isDispatcher() &&
    ['Pending', '0'].includes(status.toString());
}

canMarkAsInTransit(status: string | number): boolean {
  return this.isDispatcher() &&
    ['Prepared', '1'].includes(status.toString());
}

canDeliver(status: string | number): boolean {
  return this.isDispatcher() &&
    ['InTransit', '3'].includes(status.toString());
}

canMarkAsNotDelivered(status: string | number): boolean {
  return this.isDispatcher() &&
    ['InTransit', '3'].includes(status.toString());
}

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
}