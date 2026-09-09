import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { OrderService } from '../../services/order.service';
import { ProductService } from '../../services/product.service';
import { ShippingTypeService } from '../../services/shipping-type.service';
import { CreateOrderRequest } from '../../models/order.model';
import { ProductSummary } from '../../models/product.model';
import { ShippingTypeResponse } from '../../models/shipping-type.model';
import { HasUnsavedChanges } from '../../guards/has-unsaved-changes';

@Component({
  selector: 'app-order-create',
  standalone: false,
  templateUrl: './order-create.component.html',
  styleUrl: './order-create.component.css'
})
export class OrderCreateComponent implements OnInit, HasUnsavedChanges, OnDestroy {
  products: ProductSummary[] = [];
  shippingTypes: ShippingTypeResponse[] = [];

  private orderWasCreated = false;

  request: CreateOrderRequest = {
    customerId: '',
    shippingTypeId: '',
    street: '',
    doorNumber: '',
    apartment: '',
    lines: []
  };

  selectedProductId = '';
  selectedQuantity = 1;

  successMessage: string | null = null;
  errorMessage: string | null = null;

  private productsSubscription: Subscription | null = null;
  private shippingTypesSubscription: Subscription | null = null;

  constructor(
    private orderService: OrderService,
    private productService: ProductService,
    private shippingTypeService: ShippingTypeService
  ) {}

  ngOnInit(): void {
    this.loadLoggedUser();
    this.loadProducts();
    this.loadShippingTypes();
  }

  ngOnDestroy(): void {
    this.productsSubscription?.unsubscribe();
    this.shippingTypesSubscription?.unsubscribe();
  }

  loadLoggedUser(): void {
    const userJson = localStorage.getItem('user');

    if (!userJson) {
      this.errorMessage = 'No se encontró el usuario logueado';
      return;
    }

    const user = JSON.parse(userJson);
    this.request.customerId = user.id;
  }

  loadProducts(): void {
    this.productsSubscription = this.productService.search({ page: 1, pageSize: 100 }).subscribe({
      next: products => {
        this.products = products.filter(p => p.isActive);
      },
      error: () => {
        this.errorMessage = 'Error al cargar productos';
      }
    });
  }

  loadShippingTypes(): void {
    this.shippingTypesSubscription = this.shippingTypeService.getAll().subscribe({
      next: shippingTypes => {
        this.shippingTypes = shippingTypes;
      },
      error: () => {
        this.errorMessage = 'Error al cargar tipos de envío';
      }
    });
  }

  addLine(): void {
    if (!this.selectedProductId || this.selectedQuantity <= 0) {
      this.errorMessage = 'Seleccioná un producto y una cantidad válida';
      return;
    }

    const existingLine = this.request.lines.find(
      line => line.productId === this.selectedProductId
    );

    if (existingLine) {
      existingLine.quantity += this.selectedQuantity;
    } else {
      this.request.lines.push({
        productId: this.selectedProductId,
        quantity: this.selectedQuantity
      });
    }

    this.selectedProductId = '';
    this.selectedQuantity = 1;
    this.errorMessage = null;
  }

  changeQuantity(productId: string, delta: number): void {
    const line = this.request.lines.find(l => l.productId === productId);
    if (!line) return;

    const newQty = line.quantity + delta;
    if (newQty <= 0) {
      this.removeLine(productId);
    } else {
      line.quantity = newQty;
    }
  }

  removeLine(productId: string): void {
    this.request.lines = this.request.lines.filter(
      line => line.productId !== productId
    );
  }

  getProductName(productId: string): string {
    const product = this.products.find(p => p.id === productId);
    return product ? product.name : productId;
  }

  createOrder(): void {
    this.errorMessage = null;
    this.successMessage = null;

    if (!this.request.street) {
      this.errorMessage = 'Completá la calle';
      return;
    }

    if (!this.request.doorNumber) {
      this.errorMessage = 'Completá el número de puerta';
      return;
    }

    if (!this.request.shippingTypeId) {
      this.errorMessage = 'Seleccioná un tipo de envío';
      return;
    }

    if (this.request.lines.length === 0) {
      this.errorMessage = 'Agregá al menos un producto';
      return;
    }

    this.orderService.create(this.request).subscribe({
      next: response => {
        this.orderWasCreated = true;

        this.successMessage = `Pedido creado correctamente. Número: ${response.orderNumber}`;
        this.errorMessage = null;

        this.request.street = '';
        this.request.doorNumber = '';
        this.request.apartment = '';
        this.request.shippingTypeId = '';
        this.request.lines = [];
      },
      error: err => {
        const error = err.error;
        this.errorMessage =
          typeof error === 'string'
            ? error
            : error?.message || 'Error al crear el pedido';
        this.successMessage = null;
      }
    });
  }

  hasUnsavedChanges(): boolean {
    return !this.orderWasCreated && (
      !!this.request.street ||
      !!this.request.doorNumber ||
      !!this.request.apartment ||
      !!this.request.shippingTypeId ||
      this.request.lines.length > 0
    );
  }
}