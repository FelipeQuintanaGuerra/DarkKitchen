export interface CreateOrderLineRequest {
  productId: string;
  quantity: number;
}

export interface CreateOrderRequest {
  customerId: string;
  shippingTypeId: string;
  street: string;
  doorNumber: string;
  apartment?: string;
  lines: CreateOrderLineRequest[];
}

export interface CreateOrderResponse {
  customerId: string;
  orderNumber: string;
  subtotal: number;
  shippingCost: number;
  total: number;
}

export interface OrderLineResponse {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  discount: number;
  subtotal: number;
}

export interface OrderSummary {
  orderNumber: string;
  customerId: string;
  customerEmail: string;
  createdAt: string;
  status: string;
  total: number;
  totalProducts: number;
}

export interface OrderDetail {
  orderNumber: string;
  customerId: string;
  customerEmail: string;
  customerName: string;
  createdAt: string;
  status: string;
  shippingType: string;
  shippingCost: number;
  street: string;
  doorNumber: string;
  apartment: string;
  subtotal: number;
  total: number;
  lines: OrderLineResponse[];
}

export interface SearchCustomerOrdersRequest {
  customerId: string;
  status?: string;
  dateFrom?: string;
  dateTo?: string;
  page?: number;
  pageSize?: number;
}

export interface SearchFilteredOrdersRequest {
  dateFrom: string;
  dateTo: string;
  status?: string;
  address?: string;
  page?: number;
  pageSize?: number;
}

// Espeja el enum OrderAction del backend (Domain/Enums/OrderAction.cs)
export enum OrderAction {
  Prepare = 0,
  Cancel = 1,
  MarkAsInTransit = 2,
  Deliver = 3,
  MarkAsNotDelivered = 4,
  Delay = 5
}

export interface OrderStatusChangeResponse {
  orderNumber: string;
  status: string;
  changedAt: string;
}

