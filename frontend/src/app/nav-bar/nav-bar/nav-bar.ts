import { Component } from '@angular/core';
import { NavSearch } from '../../summoner/search/nav-search/nav-search';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-nav-bar',
  imports: [NavSearch, RouterLink],
  templateUrl: './nav-bar.html',
})
export class NavBar {}
