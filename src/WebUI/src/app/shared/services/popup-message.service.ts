import { Injectable, NgZone, inject } from '@angular/core';

type PopupType = 'success' | 'warning' | 'error' | 'info';

interface PopupAction {
  label: string;
  onClick: () => void;
}

const ICONS: Record<PopupType, string> = {
  success: 'fa-circle-check',
  warning: 'fa-triangle-exclamation',
  error: 'fa-circle-xmark',
  info: 'fa-circle-info'
};

@Injectable({
  providedIn: 'root'
})
export class PopupMessageService {
  private readonly zone = inject(NgZone);

  private defaultDuration: number = 3000;
  private messageContainer: HTMLDivElement | null = null;

  info(message: string, duration: number = this.defaultDuration): void {
    this.display(message, 'info', duration);
  }

  warning(message: string, duration: number = this.defaultDuration): void {
    this.display(message, 'warning', duration);
  }

  error(message: string, duration: number = this.defaultDuration): void {
    this.display(message, 'error', duration);
  }

  success(message: string, duration: number = this.defaultDuration): void {
    this.display(message, 'success', duration);
  }

  prompt(message: string, actionLabel: string, onClick: () => void, duration: number = 12000): void {
    this.display(message, 'info', duration, { label: actionLabel, onClick });
  }

  private display(message: string, type: PopupType, duration: number, action?: PopupAction): void {
    this.zone.run(() => {
      if (!this.messageContainer) {
        this.createMessageContainer();
      }

      this.addMessage(message, type, duration, action);
    });
  }

  private createMessageContainer() {
    this.messageContainer = document.createElement('div');
    this.messageContainer.className = 'popup-messages-container';

    document.body.appendChild(this.messageContainer);
  }

  private addMessage(message: string, type: PopupType, duration: number, action?: PopupAction): void {
    const messageElement = document.createElement('div');
    messageElement.className = `popup-message ${type}${action ? ' clickable' : ''}`;

    const icon = document.createElement('i');
    icon.className = `popup-icon fas ${ICONS[type]}`;

    const text = document.createElement('div');
    text.className = 'popup-text';
    text.textContent = message;

    if (action) {
      const link = document.createElement('span');
      link.className = 'popup-link';
      link.textContent = action.label;
      text.append(' ', link);
    }

    const closeButton = document.createElement('button');
    closeButton.type = 'button';
    closeButton.className = 'close-btn';
    closeButton.setAttribute('aria-label', 'Close');
    closeButton.textContent = '×';
    closeButton.addEventListener('click', event => {
      event.stopPropagation();
      this.removePopup(messageElement);
    });

    messageElement.append(icon, text, closeButton);
    this.messageContainer!.prepend(messageElement);

    if (action) {
      messageElement.setAttribute('role', 'button');
      messageElement.tabIndex = 0;
      messageElement.addEventListener('click', () => {
        this.removePopup(messageElement);
        this.zone.run(action.onClick);
      });
      messageElement.addEventListener('keydown', event => {
        if (event.key === 'Enter' || event.key === ' ') {
          event.preventDefault();
          messageElement.click();
        }
      });
    }

    setTimeout(() => {
      messageElement.classList.add('fade-out');
      setTimeout(() => this.removePopup(messageElement), 200);
    }, duration);
  }

  private removePopup(element: HTMLDivElement): void {
    element.remove();

    if (this.messageContainer && this.messageContainer.children.length === 0) {
      this.messageContainer.remove();
      this.messageContainer = null;
    }
  }
}
