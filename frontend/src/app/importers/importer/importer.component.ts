import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { ImporterService } from '../../services/importer.service';

@Component({
  selector: 'app-importer',
  standalone: false,
  templateUrl: './importer.component.html',
  styleUrl: './importer.component.css'
})
export class ImporterComponent implements OnInit, OnDestroy {
  importerNames: string[] = [];
  selectedImporter = '';
  filePath = '';
  successMessage: string | null = null;
  errorMessage: string | null = null;

  private namesSubscription: Subscription | null = null;
  private importSubscription: Subscription | null = null;

  constructor(private importerService: ImporterService) {}

  ngOnInit(): void {
    this.loadImporterNames();
  }

  ngOnDestroy(): void {
    this.namesSubscription?.unsubscribe();
    this.importSubscription?.unsubscribe();
  }

  loadImporterNames(): void {
    this.namesSubscription = this.importerService.getImporterNames().subscribe({
      next: (names) => {
        this.importerNames = names;
      },
      error: () => {
        this.errorMessage = 'Error al cargar los importadores disponibles';
      }
    });
  }

  import(): void {
    this.successMessage = null;
    this.errorMessage = null;

    if (!this.selectedImporter) {
      this.errorMessage = 'Seleccioná un importador';
      return;
    }

    if (!this.filePath.trim()) {
      this.errorMessage = 'Ingresá la ruta del archivo';
      return;
    }

    this.importSubscription = this.importerService.importProducts(this.selectedImporter, this.filePath).subscribe({
      next: (result) => {
        this.successMessage = `Se importaron ${result.imported} producto(s) correctamente`;
      },
      error: (err) => {
        const error = err.error;
        if (typeof error === 'string') {
          this.errorMessage = error;
        } else if (error?.message) {
          this.errorMessage = error.message;
        } else {
          this.errorMessage = 'Error al importar productos';
        }
      }
    });
  }
}
