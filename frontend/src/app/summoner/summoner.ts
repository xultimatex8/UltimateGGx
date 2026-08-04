import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class Summoner {
  private router = inject(Router);

  navigateToSummoner(username: string, tag: string) {
    this.router.navigate(['/summoner', username, tag]);
  }
}