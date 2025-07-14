import { NgOptimizedImage } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'aw-app-layout',
  imports: [RouterOutlet, NgOptimizedImage],
  templateUrl: './shared-feature-shell.html',
  styleUrl: './shared-feature-shell.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppLayoutComponent {}
