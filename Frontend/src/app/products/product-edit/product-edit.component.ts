import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { ProductService } from '../../services/product.service';
import { HasUnsavedChanges } from '../../guards/has-unsaved-changes';

@Component({
  selector: 'app-product-edit',
  standalone: false,
  templateUrl: './product-edit.component.html',
  styleUrl: './product-edit.component.css'
})
export class ProductEditComponent implements OnInit, OnDestroy, HasUnsavedChanges {
  private productWasUpdated = false;

  productForm = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.minLength(3)]),
    description: new FormControl('', [Validators.required, Validators.minLength(10)]),
    price: new FormControl('', [Validators.required, Validators.min(0.01)]),
    commercialLineName: new FormControl('', [Validators.required]),
    categoryName: new FormControl('', [Validators.required])
  });

  productId: string = '';
  errorMessage: string | null = null;
  selectedFiles: File[] = [];
  existingImages: string[] = [];
  private paramsSubscription: Subscription | null = null;

  constructor(
    private productService: ProductService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.paramsSubscription = this.route.params.subscribe((params) => {
      this.productId = params['id'];
      this.loadProduct();
    });
  }

  ngOnDestroy(): void {
    this.paramsSubscription?.unsubscribe();
  }

  loadProduct(): void {
    this.productService.getById(this.productId).subscribe({
      next: (product: any) => {
        this.productForm.patchValue({
          name: product.name,
          description: product.description,
          price: product.price,
          commercialLineName: product.commercialLineName,
          categoryName: product.categoryName
        });

        this.existingImages = product.imageUrls || [];
        this.productForm.markAsPristine();
      },
      error: () => {
        this.errorMessage = 'Error al cargar el producto';
      }
    });
  }

  onFileSelected(event: any): void {
    const files: FileList = event.target.files;
    this.selectedFiles = Array.from(files);
  }

  onSubmit(): void {
    if (!this.productForm.valid) {
      this.productForm.markAllAsTouched();
      return;
    }

    const formData = new FormData();
    formData.append('name', this.productForm.value.name!);
    formData.append('description', this.productForm.value.description!);
    formData.append('price', this.productForm.value.price!);
    formData.append('commercialLineName', this.productForm.value.commercialLineName!);
    formData.append('categoryName', this.productForm.value.categoryName!);

    this.selectedFiles.forEach((file) => {
      formData.append('images', file);
    });

    this.productService.update(this.productId, formData).subscribe({
      next: () => {
        this.productWasUpdated = true;
        this.router.navigate(['/products']);
      },
      error: (err) => {
        const error = err.error;
        if (typeof error === 'string') {
          this.errorMessage = error;
        } else if (error?.message) {
          this.errorMessage = error.message;
        } else {
          this.errorMessage = 'Error al actualizar producto';
        }
      }
    });
  }

  hasUnsavedChanges(): boolean {
    return !this.productWasUpdated && (
      this.productForm.dirty ||
      this.selectedFiles.length > 0
    );
  }
}