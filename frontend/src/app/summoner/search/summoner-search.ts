import { Injectable, inject } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Summoner } from '../summoner';

@Injectable({ providedIn: 'root' })
export class SummonerSearch {
  private fb = inject(FormBuilder);
  private summonerService = inject(Summoner);

  form = this.fb.group({
    username: [''],
    tag: ['']
  });

  searchSummoner() {
    const { username, tag } = this.form.getRawValue();
    this.summonerService.navigateToSummoner(username!, tag!);
  }
}