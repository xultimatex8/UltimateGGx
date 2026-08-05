import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';
import { MatchDto } from './match.model';
import { PagedResult } from '../../shared/models/paged-result.model';

@Injectable({ providedIn: 'root' })
export class Match {
  private http = inject(HttpClient);
  private baseUrl = '/api/matches';

  fetchSummonerMatches(puuid: string): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}/summoner/${encodeURIComponent(puuid)}`,
      null
    );
  }

  getSummonerMatches(puuid: string): Observable<PagedResult<MatchDto>> {
    return this.http.get<PagedResult<MatchDto>>(
        `${this.baseUrl}/summoner/${encodeURIComponent(puuid)}`
    );
  }
}