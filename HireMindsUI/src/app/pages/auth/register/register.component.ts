import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, FormsModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  registerForm: FormGroup;
  errorMessage: string = '';
  successMessage: string = '';
  isLoading: boolean = false;

  // OTP Flow
  showOtpScreen: boolean = false;
  otpCode: string = '';
  isVerifying: boolean = false;
  registeredEmail: string = '';

  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  constructor() {
    this.registerForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      role: ['Candidate', Validators.required]
    });
  }

  setRole(role: 'Candidate' | 'Recruiter') {
    this.registerForm.patchValue({ role });
  }

  onSubmit() {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.registeredEmail = this.registerForm.value.email;

    this.authService.register(this.registerForm.value).subscribe({
      next: (res) => {
        this.isLoading = false;
        this.successMessage = 'Registration successful! An OTP has been sent to your email.';
        this.showOtpScreen = true;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Registration failed.';
      }
    });
  }

  verifyOtp() {
    if (!this.otpCode || this.otpCode.length !== 4) {
      this.errorMessage = 'Please enter a valid 4-digit OTP.';
      return;
    }

    this.isVerifying = true;
    this.errorMessage = '';

    this.authService.verifyEmail(this.registeredEmail, this.otpCode).subscribe({
      next: (res) => {
        this.successMessage = 'Email verified successfully! Redirecting...';
        setTimeout(() => {
          if (res.role === 'Candidate') {
            this.router.navigate(['/candidate/dashboard']);
          } else {
            this.router.navigate(['/recruiter/dashboard']);
          }
        }, 1500);
      },
      error: (err) => {
        this.isVerifying = false;
        this.errorMessage = err.error?.message || 'Verification failed. Invalid or expired OTP.';
      }
    });
  }

  get f() { return this.registerForm.controls; }
}
