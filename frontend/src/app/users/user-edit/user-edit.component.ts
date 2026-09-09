import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { UserService } from '../../services/user.service';
import { HasUnsavedChanges } from '../../guards/has-unsaved-changes';

@Component({
  selector: 'app-user-edit',
  standalone: false,
  templateUrl: './user-edit.component.html',
  styleUrl: './user-edit.component.css'
})
export class UserEditComponent implements OnInit, OnDestroy, HasUnsavedChanges {
  private userWasUpdated = false;

  userForm = new FormGroup({
    name: new FormControl('', [Validators.required]),
    surname: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(25)]),
    email: new FormControl('', [Validators.required, Validators.email]),
    phone: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required, Validators.minLength(15), Validators.maxLength(25)]),
    role: new FormControl('', [Validators.required])
  });

  userId: string = '';
  errorMessage: string | null = null;
  private paramsSubscription: Subscription | null = null;

  constructor(
    private userService: UserService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.paramsSubscription = this.route.params.subscribe((params) => {
      this.userId = params['id'];
      this.loadUser();
    });
  }

  ngOnDestroy(): void {
    this.paramsSubscription?.unsubscribe();
  }

  loadUser(): void {
    this.userService.getById(this.userId).subscribe({
      next: (user) => {
        this.userForm.patchValue({
          name: user.name,
          surname: user.surname,
          email: user.email,
          phone: user.phone,
          role: String(user.role)
        });

        this.userForm.markAsPristine();
      },
      error: () => {
        this.errorMessage = 'Error al cargar el usuario';
      }
    });
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

    this.userService.update(this.userId, request).subscribe({
      next: () => {
        this.userWasUpdated = true;
        this.router.navigate(['/users']);
      },
      error: (err) => {
        const error = err.error;
        if (typeof error === 'string') {
          this.errorMessage = error;
        } else if (error?.message) {
          this.errorMessage = error.message;
        } else {
          this.errorMessage = 'Error al actualizar usuario';
        }
      }
    });
  }

  hasUnsavedChanges(): boolean {
    return !this.userWasUpdated && this.userForm.dirty;
  }
}