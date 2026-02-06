import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AdminVolunteerListItem } from '../../core/models/admin-dashboard.model';

@Component({
  selector: 'app-compose-message',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    MatAutocompleteModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './compose-message.component.html'
})
export class ComposeMessageComponent {
  @Input() isAdmin = false;
  @Input() volunteers: AdminVolunteerListItem[] = [];
  @Input() composeContent = '';
  @Input() composeVolunteerSearch = '';
  @Input() composeVolunteerId = '';
  @Input() adminTargetUserId: string | null = null;

  @Output() composeContentChange = new EventEmitter<string>();
  @Output() composeVolunteerSearchChange = new EventEmitter<string>();
  @Output() volunteerSelected = new EventEmitter<MatAutocompleteSelectedEvent>();
  @Output() send = new EventEmitter<void>();

  get sendDisabled(): boolean {
    if (!this.composeContent.trim()) return true;
    if (this.isAdmin) return !this.composeVolunteerId;
    return !this.adminTargetUserId;
  }

  getVolunteerLabel(volunteer: AdminVolunteerListItem): string {
    return `${volunteer.firstName} ${volunteer.lastName} (${volunteer.email})`;
  }
}
