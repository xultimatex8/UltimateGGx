import { Component, inject } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SummonerSearch } from '../summoner-search';

@Component({
  selector: 'app-nav-search',
  imports: [ReactiveFormsModule],
  templateUrl: './nav-search.html',
  providers: [SummonerSearch],
})
export class NavSearch {
  protected searchService = inject(SummonerSearch);
}