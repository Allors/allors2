import { Component, OnInit, Self, HostBinding, AfterViewInit, OnDestroy, Injector } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { formatDistance, format, isBefore, isAfter } from 'date-fns';

import { TestScope, MetaService, RefreshService, Action, NavigationService, PanelService, PanelManagerService, ContextService, NavigationActivatedRoute, ActionTarget } from '@allors/angular/core';
import { CommunicationEvent, ContactMechanism, CustomerShipment, ShipmentItem, Good, SalesInvoice, BillingProcess, SerialisedInventoryItemState, InventoryItem, NonSerialisedInventoryItem, Part, NonUnifiedPart, Organisation, SupplierOffering, PartyContactMechanism, PartyRate } from '@allors/domain/generated';
import { TableRow, Table, EditService, DeleteService, ObjectData, ObjectService, OverviewService } from '@allors/angular/material/core';
import { Meta } from '@allors/meta/generated';
import { ActivatedRoute } from '@angular/router';
import { InternalOrganisationId } from '@allors/angular/base';
import { PullRequest } from '@allors/protocol/system';

interface Row extends TableRow {
  object: PartyRate;
  rateType: string;
  from: string;
  through: string;
  rate: string;
  frequency: string;
}

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'partyrate-overview-panel',
  templateUrl: './partyrate-overview-panel.component.html',
  providers: [PanelService]
})
export class PartyRateOverviewPanelComponent extends TestScope implements OnInit {

  @HostBinding('class.expanded-panel') get expandedPanelClass() {
    return this.panel.isExpanded;
  }

  m: Meta;

  objects: PartyRate[];
  table: Table<Row>;

  delete: Action;
  edit: Action;

  get createData(): ObjectData {
    return {
      associationId: this.panel.manager.id,
      associationObjectType: this.panel.manager.objectType,
    };
  }

  collection = 'Current';
  currentPartyRates: PartyRate[];
  inactivePartyRates: PartyRate[];
  allPartyRates: PartyRate[] = [];

  constructor(
    @Self() public panel: PanelService,
    public metaService: MetaService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    public deleteService: DeleteService,
    public editService: EditService
  ) {
    super();

    this.m = this.metaService.m;
  }

  ngOnInit() {

    this.panel.name = 'partyrate';
    this.panel.title = 'Party Rates';
    this.panel.icon = 'contacts';
    this.panel.expandable = true;

    this.delete = this.deleteService.delete(this.panel.manager.context);
    this.edit = this.editService.edit();

    const sort = true;
    this.table = new Table({
      selection: true,
      columns: [
        { name: 'rateType' },
        { name: 'from', sort },
        { name: 'through', sort },
        { name: 'rate', sort },
        { name: 'frequency' },
      ],
      actions: [
        this.edit,
        this.delete,
      ],
      defaultAction: this.edit,
      autoSort: true,
      autoFilter: true,
    });

    const pullName = `${this.panel.name}_${this.m.PartyRate.name}`;

    this.panel.onPull = (pulls) => {

      const { pull, x, tree } = this.metaService;
      const id = this.panel.manager.id;

      pulls.push(
        pull.Party({
          name: pullName,
          object: id,
          fetch: {
            PartyRates: {
              include: {
                RateType: x,
                Frequency: x
              }
            }
          }
        }),
      );
    };

    this.panel.onPulled = (loaded) => {
      this.objects = loaded.collections[pullName] as PartyRate[];

      if (this.objects) {
        this.table.total = loaded.values[`${pullName}_total`] || this.objects.length;
        this.refreshTable();
      }
    };
  }

  public refreshTable() {
    this.table.data = this.partyRates?.map((v) => {
      return {
        object: v,
        rateType: v.RateType.Name,
        from: format(new Date(v.FromDate), 'dd-MM-yyyy'),
        through: v.ThroughDate !== null ? format(new Date(v.ThroughDate), 'dd-MM-yyyy') : '',
        rate: v.Rate,
        frequency: v.Frequency.Name,
      } as Row;
    });
  }

  get partyRates(): any {

    switch (this.collection) {
      case 'Current':
        return this.objects && this.objects.filter(v => isBefore(new Date(v.FromDate), new Date()) && (!v.ThroughDate || isAfter(new Date(v.ThroughDate), new Date())));
      case 'Inactive':
        return this.objects && this.objects.filter(v => isAfter(new Date(v.FromDate), new Date()) || (v.ThroughDate && isBefore(new Date(v.ThroughDate), new Date())));
      case 'All':
      default:
        return this.objects;
    }
  }
}
