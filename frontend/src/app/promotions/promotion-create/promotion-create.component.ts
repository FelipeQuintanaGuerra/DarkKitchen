import { Component, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { PromotionService } from '../../services/promotion.service';
import { CommercialLine } from '../../models/commercial-line.model';

@Component({
  selector: 'app-promotion-create',
  standalone: false,
  templateUrl: './promotion-create.component.html',
  styleUrl: './promotion-create.component.css'
})
export class PromotionCreateComponent implements OnInit {
  promotionForm = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.minLength(5), Validators.maxLength(25)]),
    startDate: new FormControl('', [Validators.required]),
    endDate: new FormControl('', [Validators.required]),
    discount: new FormControl('', [Validators.required, Validators.min(1), Validators.max(99)]),
    commercialLineId: new FormControl('', [Validators.required])
  });

  commercialLines: CommercialLine[] = [];
  errorMessage: string | null = null;

  constructor(
    private promotionService: PromotionService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCommercialLines();
  }

  loadCommercialLines(): void {
    this.promotionService.getCommercialLines().subscribe({
      next: (lines) => (this.commercialLines = lines),
      error: () => {
        this.errorMessage = 'Error al cargar las líneas comerciales';
      }
    });
  }

  onSubmit(): void {
    if (!this.promotionForm.valid) {
      this.promotionForm.markAllAsTouched();
      return;
    }

    this.promotionService.create({
      name: this.promotionForm.value.name!,
      startDate: this.promotionForm.value.startDate!,
      endDate: this.promotionForm.value.endDate!,
      discount: Number(this.promotionForm.value.discount!),
      commercialLineId: this.promotionForm.value.commercialLineId!
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
          this.errorMessage = 'Error al crear la promoción';
        }
      }
    });
  }
}