import { Component, OnInit } from '@angular/core';
import { PromotionService } from '../../services/promotion.service';
import { ProductService } from '../../services/product.service';
import { PromotionSummary } from '../../models/promotion.model';
import { ProductSummary } from '../../models/product.model';

@Component({
  selector: 'app-promotion-list',
  standalone: false,
  templateUrl: './promotion-list.component.html',
  styleUrl: './promotion-list.component.css'
})
export class PromotionListComponent implements OnInit {
  promotions: PromotionSummary[] = [];
  allPromotions: PromotionSummary[] = [];
  allProducts: ProductSummary[] = [];
  searchName: string = '';
  errorMessage: string | null = null;

  selectedPromotion: PromotionSummary | null = null;
  showAddProducts = false;
  modalError: string | null = null;
  userRole: string = '';

  private roleMap: { [key: number]: string } = {
    0: 'Client',
    1: 'Dispatcher',
    2: 'Admin'
  };

  constructor(
    private promotionService: PromotionService,
    private productService: ProductService
  ) {}

  ngOnInit(): void {
    this.loadUserRole();
    this.loadProducts();
    this.loadPromotions();
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

  loadProducts(): void {
    this.productService.search({}).subscribe({
      next: (products) => (this.allProducts = products),
      error: () => (this.allProducts = [])
    });
  }

  loadPromotions(): void {
    this.promotionService.getActive({}).subscribe({
      next: (promotions) => {
        this.allPromotions = promotions;
        this.applyFilter();
        this.errorMessage = null;
        if (this.selectedPromotion) {
          this.selectedPromotion =
            promotions.find(p => p.id === this.selectedPromotion!.id) ?? null;
        }
      },
      error: () => {
        this.errorMessage = 'Error al cargar las promociones';
      }
    });
  }

  search(): void {
    if (!this.searchName) {
      this.errorMessage = 'Ingresá un nombre para buscar';
      return;
    }

    this.errorMessage = null;
    this.applyFilter();
    if (this.promotions.length === 0) {
      this.errorMessage = 'No se encontraron promociones';
    }
  }

  clearSearch(): void {
    this.searchName = '';
    this.errorMessage = null;
    this.applyFilter();
  }

  private applyFilter(): void {
    const term = this.searchName.trim().toLowerCase();
    this.promotions = term
      ? this.allPromotions.filter(p => p.name.toLowerCase().includes(term))
      : this.allPromotions;
  }

  openPromotion(promotion: PromotionSummary): void {
    this.selectedPromotion = promotion;
    this.showAddProducts = false;
    this.modalError = null;
  }

  closePromotion(): void {
    this.selectedPromotion = null;
    this.showAddProducts = false;
    this.modalError = null;
  }

  toggleAddProducts(): void {
    this.showAddProducts = !this.showAddProducts;
    this.modalError = null;
  }

  // Products currently in the selected promotion (resolved to get their ids).
  get productsInPromotion(): ProductSummary[] {
    if (!this.selectedPromotion) return [];
    const names = this.selectedPromotion.productNames;
    return this.allProducts.filter(
      p =>
        p.commercialLineName === this.selectedPromotion!.commercialLineName &&
        names.includes(p.name)
    );
  }

  // Products of the same commercial line that are not yet in the promotion.
  get availableProducts(): ProductSummary[] {
    if (!this.selectedPromotion) return [];
    const names = this.selectedPromotion.productNames;
    return this.allProducts.filter(
      p =>
        p.commercialLineName === this.selectedPromotion!.commercialLineName &&
        !names.includes(p.name)
    );
  }

  addProduct(product: ProductSummary): void {
    if (!this.selectedPromotion) return;

    this.promotionService.addProduct(this.selectedPromotion.id, product.id).subscribe({
      next: () => {
        this.modalError = null;
        this.loadPromotions();
        this.showAddProducts = false;
      },
      error: (err) => (this.modalError = this.extractError(err, 'Error al agregar el producto'))
    });
  }

  removeProduct(product: ProductSummary): void {
    if (!this.selectedPromotion) return;
    if (!confirm(`¿Quitar "${product.name}" de la promoción?`)) return;

    this.promotionService.removeProduct(this.selectedPromotion.id, product.id).subscribe({
      next: () => {
        this.modalError = null;
        this.loadPromotions();
      },
      error: (err) => (this.modalError = this.extractError(err, 'Error al quitar el producto'))
    });
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString('es-UY');
  }

  private extractError(err: any, fallback: string): string {
    const error = err?.error;
    if (typeof error === 'string') return error;
    if (error?.message) return error.message;
    return fallback;
  }
}