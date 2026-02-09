import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { distinctUntilChanged, map } from 'rxjs/operators';

type ThemeMode = 'light' | 'dark';

@Injectable({
  providedIn: 'root',
})
export class AgGridThemeService {
  private readonly mode$ = new BehaviorSubject<ThemeMode>(this.getInitialMode());

  readonly themeClass$ = this.mode$.pipe(
    distinctUntilChanged(),
    map((mode) => (mode === 'dark' ? 'ag-theme-quartz-dark' : 'ag-theme-quartz')),
  );

  setDarkMode(isDark: boolean): void {
    this.mode$.next(isDark ? 'dark' : 'light');
  }

  private getInitialMode(): ThemeMode {
    const stored = typeof localStorage !== 'undefined' ? localStorage.getItem('theme') : null;
    if (stored === 'dark' || stored === 'light') {
      return stored;
    }

    if (typeof document !== 'undefined' && document.documentElement.classList.contains('dark')) {
      return 'dark';
    }

    return 'light';
  }
}
