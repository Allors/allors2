import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { MatAutocompleteModule, MatButtonModule, MatCardModule, MatIconModule, MatListModule,
         MatMenuModule, MatToolbarModule } from '@angular/material';

import { StaticModule, ChipsModule, SelectModule } from '../../../../material';

import { OrganisationsComponent } from './organisations.component';
export { OrganisationsComponent } from './organisations.component';

@NgModule({
  declarations: [
    OrganisationsComponent,
  ],
  exports: [
    OrganisationsComponent,
  ],
  imports: [
    CommonModule,
    RouterModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatListModule,
    MatMenuModule,
    MatToolbarModule,
    ChipsModule,
    StaticModule,
    SelectModule,
  ],
})
export class OrganisationsModule {}
