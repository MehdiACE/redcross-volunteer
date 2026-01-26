import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'register',
    loadComponent: () => import('./features/onboarding/registration/registration.component').then(m => m.RegistrationComponent)
  },
  {
    path: 'onboarding',
    loadComponent: () => import('./features/onboarding/stepper/stepper.component').then(m => m.StepperComponent)
  },
  {
    path: 'benevole-ponctuel',
    loadComponent: () => import('./features/onboarding/one-time-volunteer/one-time-volunteer.component').then(m => m.OneTimeVolunteerComponent)
  },
  {
    path: '',
    redirectTo: '/register',
    pathMatch: 'full'
  }
];
