import { Component, Input } from '@angular/core';

import { humanize } from '../../../../angular';
import { Action, ActionTarget } from '../../actions';
import { BaseTable } from './BaseTable';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'a-mat-table',
  templateUrl: './table.component.html',
})
export class AllorsMaterialTableComponent {
  @Input()
  public table: BaseTable;

  name(action: Action, target: ActionTarget) {
    if (action.name) {
      return action.name(target);
    }

    if (action.method) {
      return humanize(action.method.name);
    }

    return 'N/A';
  }

  handle(action: Action, target: ActionTarget) {
    action.handler(target);
  }

  disabled(action: Action, target: ActionTarget): boolean {
    if (action.method) {
      const object = target.object;
      object.canExecute(action.method.name);
    }

    return false;
  }
}
