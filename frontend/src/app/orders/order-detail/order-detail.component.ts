import { Component, Input, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { OrderService } from '../../services/order.service';
import { OrderDetail } from '../../models/order.model';

@Component({
  selector: 'app-order-detail',
  standalone: false,
  templateUrl: './order-detail.component.html',
  styleUrl: './order-detail.component.css'
})
export class OrderDetailComponent implements OnDestroy {
  @Input() order: OrderDetail | null = null;
  @Input() showSearch = true;

  orderId = '';
  errorMessage: string | null = null;

  private searchSubscription: Subscription | null = null;

  constructor(private orderService: OrderService) {}

  ngOnDestroy(): void {
    this.searchSubscription?.unsubscribe();
  }

  search(): void {
    this.errorMessage = null;
    this.order = null;

    if (!this.orderId.trim()) {
      this.errorMessage = 'Ingresá el identificador del pedido';
      return;
    }

    this.searchSubscription = this.orderService.getById(this.orderId).subscribe({
      next: order => {
        this.order = order;
      },
      error: err => {
        const error = err.error;
        this.errorMessage =
          typeof error === 'string'
            ? error
            : error?.message || 'No se encontró el pedido';
      }
    });
  }

  get subtotalWithShipping(): number {
    if (!this.order) return 0;
    return this.order.subtotal + this.order.shippingCost;
  }

  get iva(): number {
    if (!this.order) return 0;
    const value = this.order.total - (this.order.subtotal + this.order.shippingCost);
    return Math.round(value * 100) / 100;
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