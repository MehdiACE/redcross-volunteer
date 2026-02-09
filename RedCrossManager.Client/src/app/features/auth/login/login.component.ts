import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AuthService } from '../../../core/services/auth.service';
import { AgGridThemeService } from '../../../core/services/ag-grid-theme.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatIconModule,
    TranslateModule,
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  isLoading = false;
  hidePassword = true;
  currentLang: string = 'fr';
  isDarkMode = false;
  private returnUrl: string = '/onboarding';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar,
    private translate: TranslateService,
    private agGridThemeService: AgGridThemeService,
  ) {}

  ngOnInit(): void {
    const savedLang = localStorage.getItem('lang');
    this.currentLang =
      savedLang || this.translate.currentLang || this.translate.defaultLang || 'fr';
    this.translate.use(this.currentLang);

    const savedTheme = localStorage.getItem('theme');
    this.isDarkMode = savedTheme === 'dark';
    document.documentElement.classList.toggle('dark', this.isDarkMode);
    this.agGridThemeService.setDarkMode(this.isDarkMode);

    // Get the return URL from route parameters or default to '/onboarding'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/onboarding';

    // If already authenticated, redirect to admin dashboard or return URL
    if (this.authService.isAuthenticated()) {
      const target = this.authService.hasRole('Admin') ? '/admin/dashboard' : this.returnUrl;
      this.router.navigate([target]);
      return;
    }

    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]],
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const { email, password } = this.loginForm.value;

    this.authService.login(email, password).subscribe({
      next: () => {
        this.snackBar.open(
          this.translate.instant('login.success'),
          this.translate.instant('common.close'),
          { duration: 3000 },
        );
        const target = this.authService.hasRole('Admin') ? '/admin/dashboard' : this.returnUrl;
        this.router.navigate([target]);
      },
      error: (error) => {
        this.isLoading = false;
        const message =
          error.status === 401
            ? this.translate.instant('login.errors.invalidCredentials')
            : this.translate.instant('login.errors.serverError');
        this.snackBar.open(message, this.translate.instant('common.close'), { duration: 5000 });
      },
    });
  }

  getErrorMessage(fieldName: string): string {
    const control = this.loginForm.get(fieldName);
    if (!control || !control.errors) return '';

    if (control.errors['required']) {
      return this.translate.instant('login.errors.required');
    }
    if (control.errors['email']) {
      return this.translate.instant('login.errors.invalidEmail');
    }
    return '';
  }

  toggleLanguage(): void {
    this.currentLang = this.currentLang === 'fr' ? 'en' : 'fr';
    this.translate.use(this.currentLang);
    localStorage.setItem('lang', this.currentLang);
  }

  toggleTheme(): void {
    this.isDarkMode = !this.isDarkMode;
    document.documentElement.classList.toggle('dark', this.isDarkMode);
    localStorage.setItem('theme', this.isDarkMode ? 'dark' : 'light');
    this.agGridThemeService.setDarkMode(this.isDarkMode);
  }
}
