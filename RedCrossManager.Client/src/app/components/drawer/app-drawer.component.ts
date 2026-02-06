import { Component, EventEmitter, HostBinding, Input, OnChanges, OnDestroy, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-drawer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './app-drawer.component.html',
  styleUrl: './app-drawer.component.scss'
})
export class AppDrawerComponent implements OnChanges, OnDestroy {
  @Input() title = '';
  @Input() position: 'left' | 'right' = 'right';
  @Input() open = true;
  @Input() closeOnBackdrop = true;
  @Input() cancelLabel = 'Annuler';
  @Input() validateLabel = 'Valider';
  @Input() validateDisabled = false;

  @Output() closed = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();
  @Output() validated = new EventEmitter<void>();

  @HostBinding('class.is-visible')
  isVisible = false;
  isClosing = false;
  private closeTimer: ReturnType<typeof setTimeout> | null = null;
  private readonly animationDurationMs = 200;

  ngOnChanges(changes: SimpleChanges): void {
    if (!changes['open']) return;

    if (this.open) {
      this.clearCloseTimer();
      this.isVisible = true;
      this.isClosing = false;
      return;
    }

    if (this.isVisible) {
      this.isClosing = true;
      this.clearCloseTimer();
      this.closeTimer = setTimeout(() => {
        this.isVisible = false;
        this.isClosing = false;
      }, this.animationDurationMs);
    }
  }

  ngOnDestroy(): void {
    this.clearCloseTimer();
  }

  onBackdropClick(): void {
    if (!this.closeOnBackdrop) return;
    this.closed.emit();
  }

  onCloseClick(): void {
    this.closed.emit();
  }

  onCancel(): void {
    this.cancelled.emit();
    this.closed.emit();
  }

  onValidate(): void {
    if (this.validateDisabled) return;
    this.validated.emit();
    this.closed.emit();
  }

  private clearCloseTimer(): void {
    if (!this.closeTimer) return;
    clearTimeout(this.closeTimer);
    this.closeTimer = null;
  }
}
