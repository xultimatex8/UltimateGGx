import { ApplicationConfig, inject, provideAppInitializer, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { DataDragon } from './shared/data-dragon/data-dragon';
import { errorInterceptor } from './interceptors/error/error.interceptor';
import { BackendWarmup } from './shared/backend-warmup/backend-warmup';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([errorInterceptor])
    ),
    provideAppInitializer(() => {
      const warmup = inject(BackendWarmup);
      const dataDragon = inject(DataDragon);

      warmup.start().then((ready) => {
        if (ready) dataDragon.load().subscribe();
      });
    }),
  ],
};