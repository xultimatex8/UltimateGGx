import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-hero-search',
  imports: [ReactiveFormsModule],
  templateUrl: './hero-search.html',
})
export class HeroSearch {
  private fb = inject(FormBuilder);
  private router = inject(Router);

  protected form = this.fb.group({
    username: [''],
    tag: ['']
  });

  protected searchSummoner() {
    const { username, tag } = this.form.getRawValue();

    this.router.navigate(['/summoner', username, tag]);
  }
}
