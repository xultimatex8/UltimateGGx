import { Routes } from '@angular/router';
import { Home } from './home/home';
import { SummonerProfile } from './summoner/profile/summoner-profile/summoner-profile';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'summoner/:username/:tag', component: SummonerProfile },
];