import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/onboarding/registration/registration.component').then(m => m.RegistrationComponent)
  },
  {
    path: 'onboarding',
    loadComponent: () => import('./features/onboarding/stepper/stepper.component').then(m => m.StepperComponent),
    canActivate: [authGuard]
  },
  {
    path: 'benevole-ponctuel',
    loadComponent: () => import('./features/onboarding/one-time-volunteer/one-time-volunteer.component').then(m => m.OneTimeVolunteerComponent)
  },
  {
    path: '',
    redirectTo: '/login',
    pathMatch: 'full'
  }
];
