import { Component, OnInit } from '@angular/core';
import { ProductService } from '../../services/product.service';
import { ProductSummary } from '../../models/product.model';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-product-list',
  standalone: false,
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.css'
})
export class ProductListComponent implements OnInit {
  products: ProductSummary[] = [];
  searchName: string = '';
  errorMessage: string | null = null;
  userRole: string = '';

  private roleMap: { [key: number]: string } = {
    0: 'Client',
    1: 'Dispatcher',
    2: 'Admin'
  };

  constructor(private productService: ProductService) {}

  ngOnInit(): void {
    this.loadUserRole();
    this.loadProducts();
  }

  loadUserRole(): void {
    const userJson = localStorage.getItem('user');

    if (!userJson) {
      return;
    }

    const user = JSON.parse(userJson);
    const role = user.role;

    this.userRole = typeof role === 'number'
      ? this.roleMap[role] || ''
      : role || '';
  }

  isAdmin(): boolean {
    return this.userRole === 'Admin';
  }

  getImageUrl(url: string): string {
    if (url.startsWith('http')) {
      return url;
    }

    return `${environment.apiBaseUrl}${url}`;
  }

  loadProducts(): void {
    this.productService.search({}).subscribe({
      next: (products) => {
        this.products = products;
        this.errorMessage = null;
      },
      error: () => {
        this.errorMessage = 'Error al cargar los productos';
      }
    });
  }

  search(): void {
    if (!this.searchName) {
      this.errorMessage = 'Ingresá un nombre para buscar';
      return;
    }

    this.errorMessage = null;

    this.productService.search({ name: this.searchName }).subscribe({
      next: (products) => {
        this.products = products;

        if (products.length === 0) {
          this.errorMessage = 'No se encontraron productos';
        }
      },
      error: (err) => {
        const error = err.error;
        this.errorMessage =
          typeof error === 'string'
            ? error
            : error?.message || 'Error al buscar productos';
      }
    });
  }

  deleteProduct(id: string): void {
    if (!this.isAdmin()) {
      this.errorMessage = 'No tenés permisos para eliminar productos';
      return;
    }

    if (!confirm('¿Estás seguro de que querés eliminar este producto?')) {
      return;
    }

    this.productService.changeActivation(id).subscribe({
      next: (updated) => {
        this.products = this.products.map(p => p.id === id ? updated : p);
        this.errorMessage = null;
      },
      error: (err) => {
        const error = err.error;
        this.errorMessage =
          typeof error === 'string'
            ? error
            : error?.message || 'Error al eliminar producto';
      }
    });
  }

  getStatusBadgeClass(isActive: boolean): string {
    return isActive ? 'badge-active' : 'badge-inactive';
  }

  getStatusText(isActive: boolean): string {
    return isActive ? 'Activo' : 'Inactivo';
  }
}