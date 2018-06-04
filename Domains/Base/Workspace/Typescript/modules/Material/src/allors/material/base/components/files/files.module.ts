import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { MatButtonModule, MatCardModule, MatIconModule, MatInputModule } from '@angular/material';

import { AllorsMaterialFilesComponent } from './files.component';
import { FlexLayoutModule } from '@angular/flex-layout';
export { AllorsMaterialFilesComponent } from './files.component';

@NgModule({
  declarations: [
    AllorsMaterialFilesComponent,
  ],
  exports: [
    AllorsMaterialFilesComponent,
  ],
  imports: [
    CommonModule,
    FlexLayoutModule,
    FormsModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatInputModule,
  ],
})
export class AllorsMaterialFilesModule {}