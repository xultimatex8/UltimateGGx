import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

interface DataDragonVersionResponse {
  version: string;
}

@Injectable({ providedIn: 'root' })
export class DataDragon {
  private http = inject(HttpClient);

  private readonly _version = signal<string>('');
  readonly version = this._version.asReadonly();

  load(): Observable<DataDragonVersionResponse> {
    return this.http.get<DataDragonVersionResponse>('/api/datadragon/version').pipe(
      tap(({ version }) => this._version.set(version))
    );
  }
}