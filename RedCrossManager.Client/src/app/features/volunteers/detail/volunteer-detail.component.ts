import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { VolunteerService } from '../../../core/services/volunteer.service';
import { VolunteerDto } from '../../../core/models/volunteer.model';

interface VolunteerDocumentItem {
  name: string;
  type: string;
  size: string;
}

interface VolunteerMessageItem {
  from: string;
  content: string;
  timestamp: string;
  isAdmin: boolean;
}

@Component({
  selector: 'app-volunteer-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslateModule, MatSnackBarModule],
  templateUrl: './volunteer-detail.component.html'
})
export class VolunteerDetailComponent implements OnInit, OnDestroy {
  volunteer: VolunteerDto | null = null;
  isLoading = false;
  loadError = false;
  isUpdatingStatus = false;
  private volunteerId: string | null = null;
  documents: VolunteerDocumentItem[] = [];
  messages: VolunteerMessageItem[] = [];

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private volunteerService: VolunteerService,
    private snackBar: MatSnackBar,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.route.paramMap
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        const id = params.get('id');
        if (!id) return;
        this.volunteerId = id;
        this.loadVolunteer(id);
      });

    this.documents = [
      { name: 'CNI_Recto_Verso.pdf', type: 'Pièce d\'identité', size: '2.1 MB' },
      { name: 'Extrait_Casier_Judiciaire.pdf', type: 'Document légal', size: '1.3 MB' }
    ];

    this.messages = [
      {
        from: 'Volontaire',
        content: 'Bonjour, j\'ai envoyé mes documents. Pouvez-vous confirmer la réception ?',
        timestamp: '11:42',
        isAdmin: false
      },
      {
        from: 'Admin',
        content: 'Merci Jane, dossier complet. Nous revenons vers vous d\'ici 48h.',
        timestamp: '14:05',
        isAdmin: true
      }
    ];
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get fullName(): string {
    if (!this.volunteer) return '';
    return `${this.volunteer.firstName} ${this.volunteer.lastName}`.trim();
  }

  validateProfile(): void {
    if (!this.volunteerId || this.isUpdatingStatus) return;
    this.isUpdatingStatus = true;
    this.volunteerService.updateStatus(this.volunteerId, 'Active')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isUpdatingStatus = false;
          this.snackBar.open(this.translate.instant('volunteerDetail.validateSuccess'), this.translate.instant('common.close'), {
            duration: 3000,
            panelClass: ['success-snackbar']
          });
          this.loadVolunteer(this.volunteerId!);
        },
        error: () => {
          this.isUpdatingStatus = false;
        }
      });
  }

  private loadVolunteer(id: string): void {
    this.isLoading = true;
    this.loadError = false;
    this.volunteerService.getById(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: volunteer => {
          this.volunteer = volunteer;
          this.isLoading = false;
        },
        error: () => {
          this.loadError = true;
          this.isLoading = false;
        }
      });
  }
}
