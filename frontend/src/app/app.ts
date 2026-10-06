import { Component } from '@angular/core';
import {
  RouterLink,
  RouterOutlet
} from '@angular/router';

@Component({
  imports: [RouterLink,RouterOutlet],
  standalone: true,
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {
}
