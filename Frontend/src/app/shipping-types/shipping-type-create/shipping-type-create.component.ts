import { Component, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ShippingTypeService } from '../../services/shipping-type.service';

@Component({
  selector: 'app-shipping-type-create',
  standalone: false,
  templateUrl: './shipping-type-create.component.html',
  styleUrl: './shipping-type-create.component.css'
})
export class ShippingTypeCreateComponent implements OnDestroy {
  form = new FormGroup({
    name: new FormControl('', [Validators.required]),
    cost: new FormControl<number | null>(null, [Validators.required, Validators.min(0)])
  });

  errorMessage: string | null = null;

  private submitSubscription: Subscription | null = null;

  constructor(
    private shippingTypeService: ShippingTypeService,
    private router: Router
  ) {}

  ngOnDestroy(): void {
    this.submitSubscription?.unsubscribe();
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

    this.submitSubscription = this.shippingTypeService.create(request).subscribe({
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
          this.errorMessage = 'Error al crear tipo de envío';
        }
      }
    });
  }
}
