import { Component, inject } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SummonerSearch } from '../summoner-search';

@Component({
  selector: 'app-hero-search',
  imports: [ReactiveFormsModule],
  templateUrl: './hero-search.html',
  providers: [SummonerSearch],
})
export class HeroSearch {
  protected searchService = inject(SummonerSearch);
}