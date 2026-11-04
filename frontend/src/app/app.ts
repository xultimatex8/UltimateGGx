import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavBar } from './nav-bar/nav-bar/nav-bar';
import { Footer } from './footer/footer/footer';
import { BackendWakeBanner } from './shared/backend-wake-banner/backend-wake-banner';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavBar, Footer, BackendWakeBanner],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('frontend');
}
