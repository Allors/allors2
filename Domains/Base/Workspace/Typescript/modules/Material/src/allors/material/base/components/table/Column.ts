import { humanize } from '../../../../angular';

export class Column {

  name: string;
  assignedLabel: string;

  constructor(fields?: Partial<Column> | string) {

    if (typeof fields === 'string' || fields instanceof String) {
      this.name = fields as string;
    } else {
      Object.assign(this, fields);
    }
  }

  get label() {
    return this.assignedLabel || humanize(this.name);
  }
}