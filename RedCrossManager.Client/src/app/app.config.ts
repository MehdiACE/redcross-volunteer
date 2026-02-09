import { ApplicationConfig } from '@angular/core';
import { provideClientHydration } from '@angular/platform-browser';
import { appBaseConfig } from './app.config.base';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';

export const appConfig: ApplicationConfig = {
  providers: [
    ...(appBaseConfig.providers ?? []),
    provideClientHydration(),
    provideAnimationsAsync(),
  ],
};
