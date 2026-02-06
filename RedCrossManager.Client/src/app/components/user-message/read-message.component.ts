import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { MessageItem } from '../../core/models/message.model';

@Component({
  selector: 'app-read-message',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './read-message.component.html'
})
export class ReadMessageComponent {
  @Input() message: MessageItem | null = null;
}
