import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule, MatCardModule, MatDividerModule, MatFormFieldModule, MatIconModule, MatListModule, MatMenuModule, MatRadioModule, MatToolbarModule, MatTooltipModule, MatOptionModule, MatSelectModule, MatInputModule } from '@angular/material';


import { AllorsMaterialChipsModule } from '../../../../../base/components/chips';
import { AllorsMaterialDatepickerModule } from '../../../../../base/components/datepicker';
import { AllorsMaterialDatetimepickerModule } from '../../../../../base/components/datetimepicker';
import { AllorsMaterialFileModule } from '../../../../../base/components/file';
import { AllorsMaterialInputModule } from '../../../../../base/components/input';
import { AllorsMaterialSelectModule } from '../../../../../base/components/select';
import { AllorsMaterialSideNavToggleModule } from '../../../../..';
import { AllorsMaterialSlideToggleModule } from '../../../../../base/components/slidetoggle';
import { AllorsMaterialStaticModule } from '../../../../../base/components/static';
import { AllorsMaterialTextAreaModule } from '../../../../../base/components/textarea';

import { PersonInlineModule } from '../../person/person-inline.module';

import { CommunicationEventWorkTaskComponent } from './communicationevent-worktask.component';
export { CommunicationEventWorkTaskComponent } from './communicationevent-worktask.component';

@NgModule({
  declarations: [
    CommunicationEventWorkTaskComponent,
  ],
  exports: [
    CommunicationEventWorkTaskComponent,
  ],
  imports: [
    AllorsMaterialChipsModule,
    AllorsMaterialDatepickerModule,
    AllorsMaterialDatetimepickerModule,
    AllorsMaterialFileModule,
    AllorsMaterialInputModule,
    AllorsMaterialSelectModule,
    AllorsMaterialSideNavToggleModule,
    AllorsMaterialSlideToggleModule,
    AllorsMaterialStaticModule,
    AllorsMaterialTextAreaModule,

    PersonInlineModule,

    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    
    MatButtonModule,
    MatCardModule,
    MatDividerModule,
    MatFormFieldModule,  
    MatIconModule,
    MatInputModule,
    MatListModule,
    MatMenuModule,
    MatRadioModule,
    MatSelectModule,
    MatToolbarModule,
    MatTooltipModule,
    MatOptionModule,
  ],
})
export class CommunicationEventWorkTaskModule {}