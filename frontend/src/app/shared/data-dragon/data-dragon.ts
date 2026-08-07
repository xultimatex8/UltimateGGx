import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpContext } from '@angular/common/http';
import { Observable, retry, tap, timer } from 'rxjs';
import { environment } from '../../../environments/environment';
import { IS_STARTUP_REQUEST } from '../http/startup-http-context';

interface DataDragonVersionResponse {
  version: string;
}

@Injectable({ providedIn: 'root' })
export class DataDragon {
  private http = inject(HttpClient);

  private readonly baseUrl = `${environment.apiUrl}/api/datadragon`;

  private readonly _version = signal<string>('');
  readonly version = this._version.asReadonly();

  load(): Observable<DataDragonVersionResponse> {
    return this.http
      .get<DataDragonVersionResponse>(`${this.baseUrl}/version`, {
        context: new HttpContext().set(IS_STARTUP_REQUEST, true),
      })
      .pipe(
        retry({
          count: 60,
          delay: (_, retryCount) => timer(Math.min(retryCount * 1000, 5000)),
        }),
        tap(({ version }) => this._version.set(version))
      );
  }
}