import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MatIconModule } from '@angular/material/icon';
import { Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';
import { AuthService } from './core/services/auth.service';
import { AgGridThemeService } from './core/services/ag-grid-theme.service';
import { UserMessageComponent } from './components/user-message';
import { SidebarComponent } from './components/sidebar/sidebar.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, TranslateModule, UserMessageComponent, MatIconModule, SidebarComponent],
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'redcross-manager-client';
  currentLang: string = 'fr';
  isDarkMode = false;
  showUserMenu = false;
  showHeader = true;
  sidebarOpen = false;
  displayName = 'Utilisateur';
  private destroy$ = new Subject<void>();

  constructor(
    private translate: TranslateService,
    private router: Router,
    private authService: AuthService,
    private agGridThemeService: AgGridThemeService
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
    this.agGridThemeService.setDarkMode(this.isDarkMode);


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
    this.agGridThemeService.setDarkMode(this.isDarkMode);
  }

  toggleUserMenu(): void {
    this.showUserMenu = !this.showUserMenu;
  }

  toggleSidebar(): void {
    this.sidebarOpen = !this.sidebarOpen;
  }

  closeSidebar(): void {
    this.sidebarOpen = false;
  }

  logout(): void {
    this.showUserMenu = false;
    this.authService.logout();
  }

  private setDisplayName(): void {
    const storedName = localStorage.getItem('userName');
    if (storedName) {
      this.displayName = storedName;
      return;
    }
    this.displayName = 'Utilisateur';
  }

  getDisplayName(): string {
    const storedName = localStorage.getItem('userName');
    return storedName || this.displayName;
  }

  getDisplayInitials(): string {
    const name = this.getDisplayName().trim();
    if (!name) return 'U';
    const parts = name.split(' ').filter(Boolean);
    const first = parts[0]?.[0] ?? 'U';
    const second = parts.length > 1 ? parts[1]?.[0] : '';
    return `${first}${second}`.toUpperCase();
  }
}
