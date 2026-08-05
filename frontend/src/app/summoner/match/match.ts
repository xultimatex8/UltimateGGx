import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';
import { MatchDto } from './match.model';
import { PagedResult } from '../../shared/models/paged-result.model';
import { QueueType } from '../../shared/enums/queue-type';

@Injectable({ providedIn: 'root' })
export class Match {
  private http = inject(HttpClient);
  private baseUrl = '/api/matches';

  fetchSummonerMatches(
    puuid: string,
    queueType: QueueType
  ): Observable<void> {
    return this.http.post<void>(
      `${this.baseUrl}/summoner/${encodeURIComponent(puuid)}?queueType=${queueType}`,
      null
    );
  }

  getSummonerMatches(
    puuid: string,
    queueType: QueueType
  ): Observable<PagedResult<MatchDto>> {
    return this.http.get<PagedResult<MatchDto>>(
      `${this.baseUrl}/summoner/${encodeURIComponent(puuid)}?queueType=${queueType}`
    );
  }
}