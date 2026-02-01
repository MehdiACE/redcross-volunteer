import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet, NavigationEnd, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, TranslateModule],
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'redcross-manager-client';
  currentLang: string = 'fr';
  isDarkMode = false;
  showUserMenu = false;
  showHeader = true;
  displayName = 'Utilisateur';
  private destroy$ = new Subject<void>();

  constructor(
    private translate: TranslateService,
    private router: Router,
    private authService: AuthService
  ) {
    // Set default language
    this.translate.setDefaultLang('fr');
    // Use French by default
    this.translate.use('fr');
  }

  ngOnInit(): void {
    const savedLang = localStorage.getItem('lang');
    this.currentLang = savedLang || this.translate.currentLang || this.translate.defaultLang || 'fr';
    this.translate.use(this.currentLang);

    const savedTheme = localStorage.getItem('theme');
    this.isDarkMode = savedTheme === 'dark';
    document.documentElement.classList.toggle('dark', this.isDarkMode);

    this.setDisplayName();

    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        const url = this.router.url.split('?')[0];
        this.showHeader = url !== '/login' && url !== '/register';
        this.showUserMenu = false;
        this.setDisplayName();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  toggleLanguage(): void {
    this.currentLang = this.currentLang === 'fr' ? 'en' : 'fr';
    this.translate.use(this.currentLang);
    localStorage.setItem('lang', this.currentLang);
  }

  toggleTheme(): void {
    this.isDarkMode = !this.isDarkMode;
    document.documentElement.classList.toggle('dark', this.isDarkMode);
    localStorage.setItem('theme', this.isDarkMode ? 'dark' : 'light');
  }

  toggleUserMenu(): void {
    this.showUserMenu = !this.showUserMenu;
  }

  logout(): void {
    this.showUserMenu = false;
    this.authService.logout();
  }

  private setDisplayName(): void {
    const userId = this.authService.getUserId();
    const storedName = localStorage.getItem('userName');
    if (storedName) {
      this.displayName = storedName;
      return;
    }
    if (userId) {
      this.displayName = `Utilisateur ${userId.slice(0, 8)}`;
      return;
    }
    this.displayName = 'Utilisateur';
  }
}
