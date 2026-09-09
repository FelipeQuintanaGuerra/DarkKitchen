import { Component, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-register-form',
  standalone: false,
  templateUrl: './register-form.component.html',
  styleUrl: './register-form.component.css'
})
export class RegisterFormComponent implements OnDestroy {
  registerForm = new FormGroup({
    name: new FormControl('', [Validators.required]),
    surname: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(25)]),
    email: new FormControl('', [Validators.required, Validators.email]),
    phone: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required, Validators.minLength(15), Validators.maxLength(25)])
  });

  errorMessage: string | null = null;
  showPassword = false;
  successMessage: string | null = null;

  private registerSubscription: Subscription | null = null;

  constructor(
    private userService: UserService,
    private router: Router
  ) {}

  ngOnDestroy(): void {
    this.registerSubscription?.unsubscribe();
  }

  onSubmit(): void {
    if (!this.registerForm.valid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    const request = {
      name: this.registerForm.value.name!,
      surname: this.registerForm.value.surname!,
      email: this.registerForm.value.email!,
      phone: this.registerForm.value.phone!,
      password: this.registerForm.value.password!
    };

    this.registerSubscription = this.userService.register(request).subscribe({
      next: () => {
        this.successMessage = 'Registro exitoso. Redirigiendo al login...';
        this.errorMessage = null;
        setTimeout(() => this.router.navigate(['/login']), 2000);
      },
      error: (err) => {
        this.successMessage = null;
        const error = err.error;
        if (typeof error === 'string') {
          this.errorMessage = error;
        } else if (error?.message) {
          this.errorMessage = error.message;
        } else if (error?.title) {
          this.errorMessage = error.title;
        } else if (err.message) {
          this.errorMessage = err.message;
        } else {
          this.errorMessage = 'Error al registrar (código ' + err.status + '). Verificá los datos.';
        }
      }
    });
  }
}
