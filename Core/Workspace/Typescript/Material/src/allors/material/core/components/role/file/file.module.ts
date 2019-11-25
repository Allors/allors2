import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { FlexLayoutModule } from '@angular/flex-layout';

import { AllorsFocusModule } from '../../../../../angular';

import { AllorsMaterialFileComponent } from './file.component';
export { AllorsMaterialFileComponent } from './file.component';

import { FilePreviewComponent } from './preview/file-preview.component';

@NgModule({
  declarations: [
    AllorsMaterialFileComponent,
    FilePreviewComponent,
  ],
  entryComponents: [
    FilePreviewComponent,
  ],
  exports: [
    AllorsMaterialFileComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    FlexLayoutModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatInputModule,
    AllorsFocusModule
  ],
})
export class AllorsMaterialFileModule { }
