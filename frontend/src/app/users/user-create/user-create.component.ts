import { Component, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../../services/user.service';
import { HasUnsavedChanges } from '../../guards/has-unsaved-changes';

@Component({
  selector: 'app-user-create',
  standalone: false,
  templateUrl: './user-create.component.html',
  styleUrl: './user-create.component.css'
})
export class UserCreateComponent implements HasUnsavedChanges, OnDestroy {
    private userWasCreated = false;
    userForm = new FormGroup({
    name: new FormControl('', [Validators.required]),
    surname: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(25)]),
    email: new FormControl('', [Validators.required, Validators.email]),
    phone: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required, Validators.minLength(15), Validators.maxLength(25)]),
    role: new FormControl('', [Validators.required])
  });

  errorMessage: string | null = null;
  showPassword = false;

  private submitSubscription: Subscription | null = null;
  private formChangeSubscription: Subscription | null = null;

  constructor(
    private userService: UserService,
    private router: Router
  ) {
    this.formChangeSubscription = this.userForm.valueChanges.subscribe(() => {
      this.errorMessage = null;
    });
  }

  ngOnDestroy(): void {
    this.submitSubscription?.unsubscribe();
    this.formChangeSubscription?.unsubscribe();
  }

  onSubmit(): void {
    if (!this.userForm.valid) {
      this.userForm.markAllAsTouched();
      return;
    }

    const request = {
      name: this.userForm.value.name!,
      surname: this.userForm.value.surname!,
      email: this.userForm.value.email!,
      phone: this.userForm.value.phone!,
      password: this.userForm.value.password!,
      role: Number(this.userForm.value.role)
    };

    this.submitSubscription = this.userService.create(request).subscribe({
      next: () => {
        this.userWasCreated = true;
        this.router.navigate(['/users']);
      },
      error: (err) => {
        const error = err.error;
        if (typeof error === 'string') {
          this.errorMessage = error;
        } else if (error?.message) {
          this.errorMessage = error.message;
        } else {
          this.errorMessage = 'Error al crear usuario';
        }
      }
    });
  }

  hasUnsavedChanges(): boolean {
    return !this.userWasCreated && this.userForm.dirty;
  }
}