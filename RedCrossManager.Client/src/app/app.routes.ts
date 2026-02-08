import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

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
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/dashboard',
    loadComponent: () => import('./features/admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent),
    canActivate: [adminGuard]
  },
  {
    path: 'admin/volunteers/:id',
    loadComponent: () => import('./features/volunteers/detail/volunteer-detail.component').then(m => m.VolunteerDetailComponent),
    canActivate: [adminGuard]
  },
  {
    path: 'trainings',
    loadComponent: () => import('./features/trainings/trainings.component').then(m => m.TrainingsComponent),
    canActivate: [authGuard]
  },
  {
    path: 'benevole-ponctuel',
    loadComponent: () => import('./features/onboarding/one-time-volunteer/one-time-volunteer.component').then(m => m.OneTimeVolunteerComponent)
  },
  {
    path: 'guardian-consent/:volunteerId',
    loadComponent: () => import('./features/onboarding/guardian-consent/guardian-consent.component').then(m => m.GuardianConsentComponent)
  },
  {
    path: '',
    redirectTo: '/login',
    pathMatch: 'full'
  }
];
