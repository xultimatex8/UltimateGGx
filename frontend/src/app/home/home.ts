import { Component } from '@angular/core';
import { HeroSearch } from '../summoner/search/hero-search/hero-search';

@Component({
  selector: 'app-home',
  imports: [HeroSearch],
  templateUrl: './home.html',
})
export class Home {}
