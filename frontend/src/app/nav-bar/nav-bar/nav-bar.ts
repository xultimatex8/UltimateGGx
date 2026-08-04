import { Component } from '@angular/core';
import { NavSearch } from '../../summoner/search/nav-search/nav-search';

@Component({
  selector: 'app-nav-bar',
  imports: [NavSearch],
  templateUrl: './nav-bar.html',
})
export class NavBar {}
