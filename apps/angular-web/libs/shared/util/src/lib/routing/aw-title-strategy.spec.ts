import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { Title } from '@angular/platform-browser';
import { provideRouter, Router, TitleStrategy } from '@angular/router';
import { Component } from '@angular/core';
import { AwTitleStrategy } from './aw-title-strategy';

@Component({ selector: 'aw-test', template: '', standalone: true })
class TestComponent {}

describe('AwTitleStrategy', () => {
  let titleService: Title;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [
        provideRouter([
          { path: 'with-title', title: 'Dashboard', component: TestComponent },
          { path: 'no-title', component: TestComponent },
        ]),
        { provide: TitleStrategy, useClass: AwTitleStrategy },
      ],
    }).compileComponents();

    titleService = TestBed.inject(Title);
    router = TestBed.inject(Router);
  });

  it('should set title to "{page} | AdventureWorks" when route has a title', async () => {
    await router.navigateByUrl('/with-title');
    expect(titleService.getTitle()).toBe('Dashboard | AdventureWorks');
  });

  it('should set title to "AdventureWorks" when route has no title', async () => {
    await router.navigateByUrl('/no-title');
    expect(titleService.getTitle()).toBe('AdventureWorks');
  });
});
