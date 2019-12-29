import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';

import { AllorsMaterialScannerComponent } from './scanner.component';
export { AllorsMaterialScannerComponent } from './scanner.component';

@NgModule({
  declarations: [
    AllorsMaterialScannerComponent,
  ],
  exports: [
    AllorsMaterialScannerComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule
  ],
})
export class AllorsMaterialScannerModule {
}
