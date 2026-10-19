import { Component, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { SessionService } from '../../services/session.service';

@Component({
  selector: 'app-login-form',
  standalone: false,
  templateUrl: './login-form.component.html',
  styleUrl: './login-form.component.css'
})
export class LoginFormComponent implements OnDestroy {
  loginForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required])
  });

  errorMessage: string | null = null;
  showPassword = false;

  private loginSubscription: Subscription | null = null;

  constructor(
    private sessionService: SessionService,
    private router: Router
  ) {}

  ngOnDestroy(): void {
    this.loginSubscription?.unsubscribe();
  }

  onSubmit(): void {
    if (!this.loginForm.valid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    const request = {
      email: this.loginForm.value.email!,
      password: this.loginForm.value.password!
    };

    this.loginSubscription = this.sessionService.login(request).subscribe({
      next: (response) => {
        localStorage.setItem('token', response.token);
        localStorage.setItem('user', JSON.stringify(response.user));
        this.router.navigate(['/home']);
      },
      error: () => {
        this.errorMessage = 'Email o contraseña incorrectos';
      }
    });
  }
}
