import { ApplicationConfig, inject, provideAppInitializer, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { routes } from './app.routes';
import { DataDragon } from './shared/data-dragon/data-dragon';
import { errorInterceptor } from './interceptors/error/error.interceptor';
import { initializeAnalytics } from './analytics';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([errorInterceptor])
    ),
    provideAppInitializer(() => {
      initializeAnalytics();
      const dataDragon = inject(DataDragon);
      return firstValueFrom(dataDragon.load());
    }),
  ],
};