import { Injectable } from '@angular/core';
import jsPDF from 'jspdf';

export interface ChangePlatePdfData {
  userName: string;
  userId: number;
  vehicleMake: string;
  vehicleModel: string;
  vehicleYear: number;
  oldRegistration: string;
  newRegistration: string;
  reason: string;
  signatureImage: string; // Base64 PNG
  confirmationNumber?: string;
}

@Injectable({
  providedIn: 'root'
})
export class PdfGeneratorService {

  private reasonTranslations: { [key: string]: string } = {
    'Damaged': 'Oštećena tablica',
    'Lost': 'Izgubljena tablica',
    'Stolen': 'Ukrađena tablica',
    'Owner preference': 'Lična preferencija'
  };

  generateChangePlatePdf(data: ChangePlatePdfData): void {
    const doc = new jsPDF({
      orientation: 'portrait',
      unit: 'mm',
      format: 'a4'
    });

    const pageWidth = doc.internal.pageSize.getWidth();
    const pageHeight = doc.internal.pageSize.getHeight();
    let yPosition = 15;

    // Header
    doc.setFontSize(20);
    doc.setTextColor(30, 80, 180);
    doc.text('Potvrda o promeni registarske tablice', pageWidth / 2, yPosition, { align: 'center' });

    yPosition += 15;

    // Date and Confirmation Number
    doc.setFontSize(10);
    doc.setTextColor(100, 100, 100);
    const today = new Date();
    const dateStr = `${today.getDate()}.${today.getMonth() + 1}.${today.getFullYear()}`;
    const timeStr = `${today.getHours()}:${String(today.getMinutes()).padStart(2, '0')}`;
    doc.text(`Datum: ${dateStr}  Vreme: ${timeStr}`, 15, yPosition);

    if (data.confirmationNumber) {
      yPosition += 5;
      doc.text(`Referentni broj: ${data.confirmationNumber}`, 15, yPosition);
    }

    yPosition += 12;

    // Section 1: User Information
    doc.setFontSize(12);
    doc.setTextColor(30, 80, 180);
    doc.text('Podaci o korisniku', 15, yPosition);

    yPosition += 8;
    doc.setFontSize(10);
    doc.setTextColor(0, 0, 0);

    this.addInfoLine(doc, 'Ime i prezime:', data.userName, yPosition);
    yPosition += 6;
    this.addInfoLine(doc, 'ID korisnika:', String(data.userId), yPosition);

    yPosition += 12;

    // Section 2: Vehicle Information
    doc.setFontSize(12);
    doc.setTextColor(30, 80, 180);
    doc.text('Podaci o vozilu', 15, yPosition);

    yPosition += 8;
    doc.setFontSize(10);
    doc.setTextColor(0, 0, 0);

    this.addInfoLine(doc, 'Marka i model:', `${data.vehicleMake} ${data.vehicleModel}`, yPosition);
    yPosition += 6;
    this.addInfoLine(doc, 'Godina proizvodnje:', String(data.vehicleYear), yPosition);

    yPosition += 12;

    // Section 3: Change Details
    doc.setFontSize(12);
    doc.setTextColor(30, 80, 180);
    doc.text('Detalji promene', 15, yPosition);

    yPosition += 8;
    doc.setFontSize(10);
    doc.setTextColor(0, 0, 0);

    this.addInfoLine(doc, 'Trenutna tablica:', data.oldRegistration, yPosition);
    yPosition += 6;
    this.addInfoLine(doc, 'Nova tablica:', data.newRegistration, yPosition);
    yPosition += 6;
    const translatedReason = this.translateReason(data.reason);
    this.addInfoLine(doc, 'Razlog za promenu:', translatedReason, yPosition);

    yPosition += 15;

    // Section 4: Signature
    doc.setFontSize(12);
    doc.setTextColor(30, 80, 180);
    doc.text('Digitalni potpis', 15, yPosition);

    yPosition += 8;

    // Add signature image if available
    if (data.signatureImage) {
      try {
        // Signature is embedded as a small image below the label
        const signatureWidth = 60;
        const signatureHeight = 30;
        doc.addImage(
          data.signatureImage,
          'PNG',
          pageWidth / 2 - signatureWidth / 2,
          yPosition,
          signatureWidth,
          signatureHeight
        );
        yPosition += signatureHeight + 3;
      } catch (error) {
        console.error('Error adding signature to PDF:', error);
        doc.setFontSize(9);
        doc.setTextColor(200, 0, 0);
        doc.text('Potpis nije mogao biti prikazan', 15, yPosition);
        yPosition += 6;
      }
    }

    yPosition += 8;

    // Footer
    doc.setFontSize(8);
    doc.setTextColor(150, 150, 150);
    doc.text(
      'Ovaj dokument predstavlja potvrdu o promeni registarske tablice u sistemu eUprava.',
      pageWidth / 2,
      pageHeight - 10,
      { align: 'center' }
    );

    // Generate filename with timestamp
    const timestamp = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}_${String(today.getHours()).padStart(2, '0')}-${String(today.getMinutes()).padStart(2, '0')}`;
    const filename = `Promena_tablice_${data.oldRegistration}_${timestamp}.pdf`;

    // Download PDF
    doc.save(filename);
  }

  private addInfoLine(doc: jsPDF, label: string, value: string, yPosition: number): void {
    const pageWidth = doc.internal.pageSize.getWidth();
    doc.setFont('', 'bold');
    doc.text(label, 20, yPosition);
    doc.setFont('', 'normal');
    doc.text(value, 70, yPosition);
  }

  private translateReason(reason: string): string {
    return this.reasonTranslations[reason] || reason;
  }
}
