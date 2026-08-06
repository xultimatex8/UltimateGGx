import { Injectable, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Summoner } from '../summoner';

@Injectable()
export class SummonerSearch {
  private fb = inject(FormBuilder);
  private summonerService = inject(Summoner);

  form = this.fb.group({
    username: ['', [Validators.required, Validators.pattern(/.*\S.*/)]],
    tag: ['', [Validators.required, Validators.pattern(/.*\S.*/)]],
  });

  searchSummoner() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { username, tag } = this.form.getRawValue();

    this.summonerService.navigateToSummoner(username!, tag!);
  }
}