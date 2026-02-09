import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../core/services/auth.service';

interface MenuItem {
  icon: string;
  label: string;
  route: string;
  translationKey: string;
  badge?: number;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, MatIconModule, TranslateModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
})
export class SidebarComponent {
  @Input() isOpen = false;
  @Output() closed = new EventEmitter<void>();

  menuItems: MenuItem[] = [
    {
      icon: 'dashboard',
      label: 'Dashboard',
      route: '/dashboard',
      translationKey: 'sidebar.dashboard',
    },
    {
      icon: 'person',
      label: 'Mon Profil',
      route: '/profile',
      translationKey: 'sidebar.profile',
    },
    {
      icon: 'assignment',
      label: 'Mes Missions',
      route: '/missions',
      translationKey: 'sidebar.missions',
    },
    {
      icon: 'folder',
      label: 'Mes Documents',
      route: '/documents',
      translationKey: 'sidebar.documents',
    },
    {
      icon: 'mail',
      label: 'Messagerie',
      route: '/messages',
      translationKey: 'sidebar.messages',
      badge: 0,
    },
    {
      icon: 'school',
      label: 'Formations',
      route: '/trainings',
      translationKey: 'sidebar.trainings',
    },
  ];

  userInitials = '';
  userRole = '';
  userName = '';

  constructor(
    private router: Router,
    private authService: AuthService,
  ) {
    this.initializeUser();
  }

  private initializeUser(): void {
    const userName = localStorage.getItem('userName') || '';
    const roles = this.authService.getRoles();

    this.userName = userName;
    this.userRole = roles.length > 0 ? roles[0] : '';
    this.userInitials =
      this.userName
        .split(' ')
        .map((n) => n[0])
        .join('')
        .toUpperCase()
        .slice(0, 2) || 'U';

    // Show admin-specific menu items
    if (this.authService.hasRole('Admin')) {
      this.menuItems.splice(1, 0, {
        icon: 'supervised_user_circle',
        label: 'Volontaires',
        route: '/admin/dashboard',
        translationKey: 'sidebar.volunteers',
      });
    }
  }

  navigateTo(route: string): void {
    this.router.navigate([route]);
    this.closeSidebar();
  }

  closeSidebar(): void {
    this.closed.emit();
  }

  onBackdropClick(): void {
    this.closeSidebar();
  }
}
