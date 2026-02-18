import { Component, OnInit, AfterViewInit, inject, ViewChild, ElementRef, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import SignaturePad from 'signature_pad';

@Component({
  selector: 'app-signature-pad',
  standalone: true,
  imports: [CommonModule, ButtonModule, MessageModule],
  template: `
    <div class="space-y-4">
      <div>
        <label class="block text-sm font-medium text-gray-700 mb-2">
          Ваш потпис *
        </label>
        <div class="border-2 border-dashed border-gray-300 rounded-lg bg-white p-2">
          <canvas
            #canvas
            class="w-full border border-gray-300 rounded cursor-crosshair bg-white"
            [ngStyle]="{ 'min-height': '200px' }"
          ></canvas>
        </div>
        <small class="text-gray-500 block mt-1">Потпишите у поље изнад</small>
      </div>

      <div class="flex gap-2">
        <p-button
          label="Обриши потпис"
          icon="pi pi-trash"
          severity="secondary"
          (onClick)="clearSignature()"
          [disabled]="!isDrawn()"
        ></p-button>
        <p-button
          label="Потврди потпис"
          icon="pi pi-check"
          (onClick)="confirmSignature()"
          [disabled]="!isDrawn()"
        ></p-button>
      </div>

      @if (isConfirmed()) {
        <p-message
          severity="success"
          text="Потпис је потврђен"
        ></p-message>
      }

      @if (error()) {
        <p-message
          severity="error"
          [text]="error()"
        ></p-message>
      }
    </div>
  `,
  styles: [`
    :host ::ng-deep {
      .p-button {
        width: 100%;
      }
    }
  `]
})
export class SignaturePadComponent implements OnInit, AfterViewInit {
  @ViewChild('canvas') canvasElement!: ElementRef<HTMLCanvasElement>;
  @Output() signatureCaptured = new EventEmitter<string>();

  signaturePad!: SignaturePad;
  isDrawn = signal(false);
  isConfirmed = signal(false);
  error = signal<string>('');

  ngOnInit(): void {
    // Component initialization
  }

  ngAfterViewInit(): void {
    this.resizeCanvas();
    this.initializeSignaturePad();
    window.addEventListener('resize', () => this.resizeCanvas());
  }

  initializeSignaturePad(): void {
    const canvas = this.canvasElement.nativeElement;
    this.signaturePad = new SignaturePad(canvas, {
      minWidth: 2,
      maxWidth: 3,
      penColor: '#000000',
      backgroundColor: '#FFFFFF'
    });

    // Track drawing on the signature pad using canvas events
    canvas.addEventListener('mousedown', () => {
      setTimeout(() => {
        this.isDrawn.set(!this.signaturePad.isEmpty());
      }, 0);
    });

    canvas.addEventListener('touchstart', () => {
      setTimeout(() => {
        this.isDrawn.set(!this.signaturePad.isEmpty());
      }, 0);
    });

    canvas.addEventListener('mouseup', () => {
      this.isDrawn.set(!this.signaturePad.isEmpty());
    });

    canvas.addEventListener('touchend', () => {
      this.isDrawn.set(!this.signaturePad.isEmpty());
    });
  }

  resizeCanvas(): void {
    if (!this.canvasElement) return;

    const canvas = this.canvasElement.nativeElement;
    const ratio = Math.max(window.devicePixelRatio || 1, 1);
    canvas.width = canvas.offsetWidth * ratio;
    canvas.height = canvas.offsetHeight * ratio;
    canvas.getContext('2d')!.scale(ratio, ratio);

    // Redraw signature after resize
    if (this.signaturePad) {
      this.signaturePad.clear();
      this.isDrawn.set(false);
      this.isConfirmed.set(false);
    }
  }

  clearSignature(): void {
    if (this.signaturePad) {
      this.signaturePad.clear();
      this.isDrawn.set(false);
      this.isConfirmed.set(false);
      this.error.set('');
      this.signatureCaptured.emit('');
    }
  }

  confirmSignature(): void {
    if (!this.signaturePad || this.signaturePad.isEmpty()) {
      this.error.set('Молим вас да потпишете пре него што наставите');
      return;
    }

    // Convert signature to Base64 PNG
    const signatureDataUrl = this.signaturePad.toDataURL();
    this.signatureCaptured.emit(signatureDataUrl);
    this.isConfirmed.set(true);
    this.error.set('');
  }

  getSignatureData(): string | null {
    if (!this.signaturePad || this.signaturePad.isEmpty()) {
      return null;
    }
    return this.signaturePad.toDataURL();
  }

  reset(): void {
    this.clearSignature();
  }
}
