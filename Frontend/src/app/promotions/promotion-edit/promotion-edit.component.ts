import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { PromotionService } from '../../services/promotion.service';

@Component({
  selector: 'app-promotion-edit',
  standalone: false,
  templateUrl: './promotion-edit.component.html',
  styleUrl: './promotion-edit.component.css'
})
export class PromotionEditComponent implements OnInit, OnDestroy {
  promotionForm = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.minLength(5), Validators.maxLength(25)]),
    startDate: new FormControl('', [Validators.required]),
    endDate: new FormControl('', [Validators.required]),
    discount: new FormControl('', [Validators.required, Validators.min(1), Validators.max(99)])
  });

  promotionId: string = '';
  errorMessage: string | null = null;
  private paramsSubscription: Subscription | null = null;

  constructor(
    private promotionService: PromotionService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.paramsSubscription = this.route.params.subscribe((params) => {
      this.promotionId = params['id'];
      this.loadPromotion();
    });
  }

  ngOnDestroy(): void {
    this.paramsSubscription?.unsubscribe();
  }

  loadPromotion(): void {
    this.promotionService.getActive({}).subscribe({
      next: (promotions) => {
        const promotion = promotions.find(p => p.id === this.promotionId);
        if (!promotion) {
          this.errorMessage = 'No se encontró la promoción';
          return;
        }
        this.promotionForm.patchValue({
          name: promotion.name,
          startDate: this.toDateInput(promotion.startDate),
          endDate: this.toDateInput(promotion.endDate),
          discount: promotion.discount.toString()
        });
      },
      error: () => {
        this.errorMessage = 'Error al cargar la promoción';
      }
    });
  }

  onSubmit(): void {
    if (!this.promotionForm.valid) {
      this.promotionForm.markAllAsTouched();
      return;
    }

    this.promotionService.update(this.promotionId, {
      name: this.promotionForm.value.name!,
      startDate: this.promotionForm.value.startDate!,
      endDate: this.promotionForm.value.endDate!,
      discount: Number(this.promotionForm.value.discount!)
    }).subscribe({
      next: () => {
        this.router.navigate(['/promotions']);
      },
      error: (err) => {
        const error = err.error;
        if (typeof error === 'string') {
          this.errorMessage = error;
        } else if (error?.message) {
          this.errorMessage = error.message;
        } else {
          this.errorMessage = 'Error al actualizar la promoción';
        }
      }
    });
  }

  // ISO date -> yyyy-MM-dd for <input type="date">
  private toDateInput(date: string): string {
    return new Date(date).toISOString().split('T')[0];
  }
}