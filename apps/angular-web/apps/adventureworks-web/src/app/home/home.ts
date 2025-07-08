import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'aw-home',
  standalone: true,
  templateUrl: './home.html',
  styleUrl: './home.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent {}
