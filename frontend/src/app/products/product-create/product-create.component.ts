import { Component } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { HasUnsavedChanges } from '../../guards/has-unsaved-changes';

@Component({
  selector: 'app-product-create',
  standalone: false,
  templateUrl: './product-create.component.html',
  styleUrl: './product-create.component.css'
})
export class ProductCreateComponent implements HasUnsavedChanges {
  private productWasCreated = false;

  productForm = new FormGroup({
    code: new FormControl('', [Validators.required]),
    name: new FormControl('', [Validators.required, Validators.minLength(3)]),
    description: new FormControl('', [Validators.required, Validators.minLength(10)]),
    price: new FormControl('', [Validators.required, Validators.min(0.01)]),
    commercialLineName: new FormControl('', [Validators.required]),
    categoryName: new FormControl('', [Validators.required])
  });

  selectedFiles: File[] = [];
  errorMessage: string | null = null;

  constructor(
    private productService: ProductService,
    private router: Router
  ) {}

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
    formData.append('code', this.productForm.value.code!);
    formData.append('name', this.productForm.value.name!);
    formData.append('description', this.productForm.value.description!);
    formData.append('price', this.productForm.value.price!);
    formData.append('commercialLineName', this.productForm.value.commercialLineName!);
    formData.append('categoryName', this.productForm.value.categoryName!);

    this.selectedFiles.forEach(file => {
      formData.append('images', file);
    });

    this.productService.create(formData).subscribe({
      next: () => {
        this.productWasCreated = true;
        this.router.navigate(['/products']);
      },
      error: (err) => {
        const error = err.error;
        if (typeof error === 'string') {
          this.errorMessage = error;
        } else if (error?.message) {
          this.errorMessage = error.message;
        } else {
          this.errorMessage = 'Error al crear producto';
        }
      }
    });
  }

  hasUnsavedChanges(): boolean {
    return !this.productWasCreated && (
      this.productForm.dirty ||
      this.selectedFiles.length > 0
    );
  }
}