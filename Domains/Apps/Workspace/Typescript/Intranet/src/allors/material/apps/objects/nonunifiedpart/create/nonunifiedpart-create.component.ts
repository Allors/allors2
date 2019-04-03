import { Component, OnDestroy, OnInit, Self, Inject, Optional } from '@angular/core';

import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import {  ContextService, MetaService, RefreshService } from '../../../../../angular';
import { Facility, Locale, Organisation, Part, InventoryItemKind, ProductType, SupplierOffering, Brand, Model, ProductIdentificationType, PartNumber, UnitOfMeasure, Settings, SupplierRelationship, NonUnifiedPart } from '../../../../../domain';
import { Equals, PullRequest, Sort, IObject } from '../../../../../framework';
import { CreateData, SaveService } from '../../../../../material';
import { Meta } from '../../../../../meta';
import { StateService } from '../../../services/state';
import { Fetcher } from '../../Fetcher';
import { MatDialogRef, MAT_DIALOG_DATA, MatSnackBar } from '@angular/material';

@Component({
  templateUrl: './nonunifiedpart-create.component.html',
  providers: [ContextService]
})
export class NonUnifiedPartCreateComponent implements OnInit, OnDestroy {

  readonly m: Meta;

  public title = 'Add Part';

  part: Part;
  facility: Facility;
  locales: Locale[];
  supplierRelationships: SupplierRelationship[];
  inventoryItemKinds: InventoryItemKind[];
  productTypes: ProductType[];
  manufacturers: Organisation[];
  suppliers: Organisation[];
  selectedSuppliers: Organisation[];
  supplierOfferings: SupplierOffering[];
  brands: Brand[];
  selectedBrand: Brand;
  models: Model[];
  selectedModel: Model;
  organisations: Organisation[];
  addBrand = false;
  addModel = false;
  goodIdentificationTypes: ProductIdentificationType[];
  partNumber: PartNumber;
  facilities: Facility[];
  unitsOfMeasure: UnitOfMeasure[];
  settings: Settings;
  currentSuppliers: Set<Organisation>;

  private subscription: Subscription;
  private fetcher: Fetcher;

  constructor(
    @Self() public allors: ContextService,
    @Optional() @Inject(MAT_DIALOG_DATA) public data: CreateData,
    public dialogRef: MatDialogRef<NonUnifiedPartCreateComponent>,
    public metaService: MetaService,
    private refreshService: RefreshService,
    private saveService: SaveService,
    public stateService: StateService,
    private snackBar: MatSnackBar) {

    this.m = this.metaService.m;
    this.fetcher = new Fetcher(this.stateService, this.metaService.pull);
  }

  public ngOnInit(): void {

    const { m, pull, x } = this.metaService;

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(([]) => {

          const pulls = [
            this.fetcher.locales,
            this.fetcher.Settings,
            pull.Facility(),
            pull.UnitOfMeasure(),
            pull.InventoryItemKind(),
            pull.ProductIdentificationType(),
            pull.Ownership({ sort: new Sort(m.Ownership.Name) }),
            pull.ProductType({ sort: new Sort(m.ProductType.Name) }),
            pull.SupplierRelationship({
              include: {
                Supplier: x
              }
            }),
            pull.Brand({
              include: {
                Models: x
              },
              sort: new Sort(m.Brand.Name)
            }),
            pull.Organisation({
              predicate: new Equals({ propertyType: m.Organisation.IsManufacturer, value: true }),
            }),
          ];

          return this.allors.context
            .load('Pull', new PullRequest({ pulls }));
        })
      )
      .subscribe((loaded) => {

        this.allors.context.reset();

        const now = new Date();

        this.inventoryItemKinds = loaded.collections.InventoryItemKinds as InventoryItemKind[];
        this.productTypes = loaded.collections.ProductTypes as ProductType[];
        this.brands = loaded.collections.Brands as Brand[];
        this.locales = loaded.collections.AdditionalLocales as Locale[];
        this.facilities = loaded.collections.Facilities as Facility[];
        this.manufacturers = loaded.collections.Organisations as Organisation[];
        this.settings = loaded.objects.Settings as Settings;

        const supplierRelationships = loaded.collections.SupplierRelationships as SupplierRelationship[];
        const currentsupplierRelationships = supplierRelationships.filter(v => v.FromDate <= now && (v.ThroughDate === null || v.ThroughDate >= now));
        this.currentSuppliers = new Set(currentsupplierRelationships.map(v => v.Supplier).sort((a, b) => (a.Name > b.Name) ? 1 : ((b.Name > a.Name) ? -1 : 0)));

        this.unitsOfMeasure = loaded.collections.UnitsOfMeasure as UnitOfMeasure[];
        const piece = this.unitsOfMeasure.find((v) => v.UniqueId.toUpperCase() === 'F4BBDB52-3441-4768-92D4-729C6C5D6F1B');

        this.goodIdentificationTypes = loaded.collections.ProductIdentificationTypes as ProductIdentificationType[];
        const partNumberType = this.goodIdentificationTypes.find((v) => v.UniqueId.toUpperCase() === '5735191A-CDC4-4563-96EF-DDDC7B969CA6');

        this.manufacturers = loaded.collections.Organisations as Organisation[];

        this.part = this.allors.context.create('NonUnifiedPart') as NonUnifiedPart;
        this.part.DefaultFacility = this.settings.DefaultFacility;
        this.part.UnitOfMeasure = piece;

        if (!this.settings.UsePartNumberCounter) {
          this.partNumber = this.allors.context.create('PartNumber') as PartNumber;
          this.partNumber.ProductIdentificationType = partNumberType;

          this.part.AddProductIdentification(this.partNumber);
        }
      });
  }

  public brandAdded(brand: Brand): void {
    this.brands.push(brand);
    this.selectedBrand = brand;
    this.models = [];
    this.selectedModel = undefined;
  }

  public modelAdded(model: Model): void {
    this.selectedBrand.AddModel(model);
    this.models = this.selectedBrand.Models.sort((a, b) => (a.Name > b.Name) ? 1 : ((b.Name > a.Name) ? -1 : 0));
    this.selectedModel = model;
  }

  public brandSelected(brand: Brand): void {

    const { pull, x } = this.metaService;

    const pulls = [
      pull.Brand({
        object: brand,
        include: {
          Models: x,
        }
      }
      )
    ];

    this.allors.context
      .load('Pull', new PullRequest({ pulls }))
      .subscribe(() => {
        this.models = this.selectedBrand.Models ? this.selectedBrand.Models.sort((a, b) => (a.Name > b.Name) ? 1 : ((b.Name > a.Name) ? -1 : 0)) : [];
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {

    this.onSave();

    this.allors.context
      .save()
      .subscribe(() => {
        const data: IObject = {
          id: this.part.id,
          objectType: this.part.objectType,
        };

        this.dialogRef.close(data);
      },
      this.saveService.errorHandler
    );
  }

  public update(): void {
    const { context } = this.allors;

    this.onSave();

    context
      .save()
      .subscribe(() => {
        this.snackBar.open('Successfully saved.', 'close', { duration: 5000 });
        this.refreshService.refresh();
      },
      this.saveService.errorHandler
    );
  }

  private onSave() {

    this.part.Brand = this.selectedBrand;
    this.part.Model = this.selectedModel;

    if (this.selectedSuppliers !== undefined) {
      this.selectedSuppliers.forEach((supplier: Organisation) => {
        const supplierOffering = this.allors.context.create('SupplierOffering') as SupplierOffering;
        supplierOffering.Supplier = supplier;
        supplierOffering.Part = this.part;
        supplierOffering.UnitOfMeasure = this.part.UnitOfMeasure;
        supplierOffering.Currency = this.settings.PreferredCurrency;
      });
    }
  }
}
