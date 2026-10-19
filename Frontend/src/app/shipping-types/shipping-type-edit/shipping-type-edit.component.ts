import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { ShippingTypeService } from '../../services/shipping-type.service';

@Component({
  selector: 'app-shipping-type-edit',
  standalone: false,
  templateUrl: './shipping-type-edit.component.html',
  styleUrl: './shipping-type-edit.component.css'
})
export class ShippingTypeEditComponent implements OnInit, OnDestroy {
  form = new FormGroup({
    name: new FormControl('', [Validators.required]),
    cost: new FormControl<number | null>(null, [Validators.required, Validators.min(0)])
  });

  shippingTypeId: string = '';
  errorMessage: string | null = null;
  private paramsSubscription: Subscription | null = null;

  constructor(
    private shippingTypeService: ShippingTypeService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.paramsSubscription = this.route.params.subscribe((params) => {
      this.shippingTypeId = params['id'];
      this.loadShippingType();
    });
  }

  ngOnDestroy(): void {
    this.paramsSubscription?.unsubscribe();
  }

  loadShippingType(): void {
    this.shippingTypeService.getAll().subscribe({
      next: (types) => {
        const type = types.find(t => t.id === this.shippingTypeId);
        if (type) {
          this.form.patchValue({
            name: type.name,
            cost: type.cost
          });
        } else {
          this.errorMessage = 'Tipo de envío no encontrado';
        }
      },
      error: () => {
        this.errorMessage = 'Error al cargar el tipo de envío';
      }
    });
  }

  onSubmit(): void {
    if (!this.form.valid) {
      this.form.markAllAsTouched();
      return;
    }

    const request = {
      name: this.form.value.name!,
      cost: this.form.value.cost!
    };

    this.shippingTypeService.update(this.shippingTypeId, request).subscribe({
      next: () => {
        this.router.navigate(['/shipping-types']);
      },
      error: (err) => {
        const error = err.error;
        if (typeof error === 'string') {
          this.errorMessage = error;
        } else if (error?.message) {
          this.errorMessage = error.message;
        } else {
          this.errorMessage = 'Error al actualizar tipo de envío';
        }
      }
    });
  }
}
