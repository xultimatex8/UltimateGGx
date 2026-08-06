import { Routes } from '@angular/router';
import { Home } from './home/home';
import { SummonerProfile } from './summoner/profile/summoner-profile/summoner-profile';
import { MatchTimeline } from './timeline/match-timeline/match-timeline';
import { Terms } from './terms/terms';
import { NotFound } from './pages/not-found/not-found';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'summoner/:username/:tag', component: SummonerProfile },
  { path: 'match/:matchId', component: MatchTimeline },
  { path: 'terms', component: Terms },
  { path: '**', component: NotFound },
];