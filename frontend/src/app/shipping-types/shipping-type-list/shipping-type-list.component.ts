import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { ShippingTypeService } from '../../services/shipping-type.service';
import { ShippingTypeResponse } from '../../models/shipping-type.model';

@Component({
  selector: 'app-shipping-type-list',
  standalone: false,
  templateUrl: './shipping-type-list.component.html',
  styleUrl: './shipping-type-list.component.css'
})
export class ShippingTypeListComponent implements OnInit, OnDestroy {
  shippingTypes: ShippingTypeResponse[] = [];
  errorMessage: string | null = null;

  private loadSubscription: Subscription | null = null;

  constructor(private shippingTypeService: ShippingTypeService) {}

  ngOnInit(): void {
    this.loadShippingTypes();
  }

  ngOnDestroy(): void {
    this.loadSubscription?.unsubscribe();
  }

  loadShippingTypes(): void {
    this.loadSubscription = this.shippingTypeService.getAll().subscribe({
      next: (types) => {
        this.shippingTypes = types;
      },
      error: () => {
        this.errorMessage = 'Error al cargar los tipos de envío';
      }
    });
  }

  deleteShippingType(id: string): void {
    if (!confirm('¿Estás seguro de que querés eliminar este tipo de envío?')) {
      return;
    }

    this.errorMessage = null;
    this.shippingTypeService.delete(id).subscribe({
      next: () => {
        this.shippingTypes = this.shippingTypes.filter(t => t.id !== id);
      },
      error: (err) => {
        const error = err.error;
        if (typeof error === 'string') {
          this.errorMessage = error;
        } else if (error?.message) {
          this.errorMessage = error.message;
        } else {
          this.errorMessage = 'Error al eliminar el tipo de envío';
        }
      }
    });
  }
}
