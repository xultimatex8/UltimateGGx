import { Routes } from '@angular/router';
import { Home } from './home/home';
import { SummonerProfile } from './summoner/profile/summoner-profile/summoner-profile';
import { MatchTimeline } from './timeline/match-timeline/match-timeline';
import { Terms } from './terms/terms';
import { NotFound } from './pages/not-found/not-found';
import { ServerError } from './pages/server-error/server-error';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'summoner/:username/:tag', component: SummonerProfile },
  { path: 'match/:matchId', component: MatchTimeline },
  { path: 'terms', component: Terms },
  { path: '500', component: ServerError },
  { path: '**', component: NotFound },
];