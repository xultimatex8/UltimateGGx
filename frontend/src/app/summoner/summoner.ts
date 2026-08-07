import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs/internal/Observable';
import { SummonerDto } from './summoner.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class Summoner {
  private router = inject(Router);
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/api/summoners`;

  navigateToSummoner(username: string, tag: string) {
    this.router.navigate(['/summoner', username, tag]);
  }

  getSummoner(username: string, tag: string): Observable<SummonerDto> {
    return this.http.get<SummonerDto>(
      `${this.baseUrl}/${encodeURIComponent(username)}/${encodeURIComponent(tag)}`
    );
  }

  refreshSummoner(username: string, tag: string): Observable<SummonerDto> {
    return this.http.put<SummonerDto>(
      `${this.baseUrl}/${encodeURIComponent(username)}/${encodeURIComponent(tag)}/refresh`,
      null
    );
  }
}