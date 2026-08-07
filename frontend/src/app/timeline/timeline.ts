import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ScoreboardDto, TimelineDto } from './timeline.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class Timeline {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/api/timelines`;

  getMatchTimeline(matchId: string): Observable<TimelineDto> {
    return this.http.get<TimelineDto>(
      `${this.baseUrl}/match/${encodeURIComponent(matchId)}`
    );
  }

  getMatchScoreboard(
    matchId: string,
    timestamp?: number
  ): Observable<ScoreboardDto> {
    let params = new HttpParams();
    if (timestamp !== undefined && timestamp !== null) {
      params = params.set('timestamp', timestamp.toString());
    }

    return this.http.get<ScoreboardDto>(
      `${this.baseUrl}/match/${encodeURIComponent(matchId)}/scoreboard`,
      { params }
    );
  }
}