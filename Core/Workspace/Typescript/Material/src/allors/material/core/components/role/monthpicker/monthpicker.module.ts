import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';

import { AllorsFocusModule } from '../../../../../angular';

import { AllorsMaterialMonthpickerComponent } from './monthpicker.component';
export { AllorsMaterialMonthpickerComponent } from './monthpicker.component';

@NgModule({
  declarations: [
    AllorsMaterialMonthpickerComponent,
  ],
  exports: [
    AllorsMaterialMonthpickerComponent,
  ],
  imports: [
    FormsModule,
    CommonModule,
    MatInputModule,
    MatIconModule,
    MatDatepickerModule,
    AllorsFocusModule
  ],
 })
export class AllorsMaterialMonthpickerModule {
}
