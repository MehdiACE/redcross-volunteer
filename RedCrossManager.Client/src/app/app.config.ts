import { ApplicationConfig } from '@angular/core';
import { provideClientHydration } from '@angular/platform-browser';
import { appBaseConfig } from './app.config.base';

export const appConfig: ApplicationConfig = {
  providers: [
    ...(appBaseConfig.providers ?? []),
    provideClientHydration()
  ]
};
