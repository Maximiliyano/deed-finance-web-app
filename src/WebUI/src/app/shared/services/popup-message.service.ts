import { Injectable, NgZone } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class PopupMessageService {
  private defaultDuration: number = 3000;
  private messageContainer: HTMLDivElement | null = null;

  constructor(private readonly zone: NgZone) {}

  warning(message: string, duration: number = this.defaultDuration): void {
    this.display(message, 'warning', duration);
  }

  error(message: string, duration: number = this.defaultDuration): void {
    this.display(message, 'error', duration);
  }

  success(message: string, duration: number = this.defaultDuration): void {
    this.display(message, 'success', duration);
  }

  private display(message: string, type: 'success' | 'warning' | 'error', duration: number): void {
    this.zone.run(() => {
      if (!this.messageContainer) {
        this.createMessageContainer();
      }

      this.addMessage(message, type, duration);
    });
  }

  private createMessageContainer() {
    this.messageContainer = document.createElement('div');
    this.messageContainer.className = 'popup-messages-container';

    document.body.appendChild(this.messageContainer);
  }

  private addMessage(message: string, type: 'success' | 'warning' | 'error', duration: number): void {
    const messageElement = document.createElement('div');

    messageElement.className = `popup-message ${type}`;
    messageElement.innerHTML = `
      <div class="popup-icon">${this.getIcon(type)}</div>
      <div class="popup-text">${message}</div>
      <button class="close-btn">&times;</button>
    `;

    this.messageContainer!.prepend(messageElement);

    const closeButton = messageElement.querySelector('.close-btn');

    if (closeButton) {
      closeButton.addEventListener('click', () => this.removePopup(messageElement));
    }

    setTimeout(() => {
      messageElement.classList.add('fade-out');

      setTimeout(() => {
        this.removePopup(messageElement);
      }, duration);
    }, duration)
  }

  private getIcon(type: 'success' | 'warning' | 'error'): string {
    switch (type) {
      case 'success':
        return '✔️';
      case 'warning':
        return '⚠️';
      case 'error':
        return '❌';
      default:
        return '';
    }
  }

  private removePopup(element: HTMLDivElement): void {
    element.remove();

    if (this.messageContainer && this.messageContainer.children.length === 0) {
      this.messageContainer.remove();
      this.messageContainer = null;
    }
  }
}
