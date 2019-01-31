import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Subscription, combineLatest, BehaviorSubject } from 'rxjs';
import { switchMap, filter } from 'rxjs/operators';

import { ErrorService, ContextService, NavigationService, PanelService, RefreshService, MetaService } from '../../../../../../angular';
import { Locale, Organisation, Good, Facility, ProductCategory, ProductType, Brand, Model, VendorProduct, Ownership, VatRate, Part, GoodIdentificationType, ProductNumber } from '../../../../../../domain';
import { PullRequest, Sort } from '../../../../../../framework';
import { Meta } from '../../../../../../meta';
import { StateService } from '../../../../services/state';
import { Fetcher } from '../../../Fetcher';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'good-overview-detail',
  templateUrl: './good-overview-detail.component.html',
  providers: [PanelService, ContextService]
})
export class GoodOverviewDetailComponent implements OnInit, OnDestroy {

  readonly m: Meta;

  good: Good;

  facility: Facility;
  locales: Locale[];
  categories: ProductCategory[];
  productTypes: ProductType[];
  manufacturers: Organisation[];
  brands: Brand[];
  selectedBrand: Brand;
  models: Model[];
  selectedModel: Model;
  vendorProduct: VendorProduct;
  vatRates: VatRate[];
  ownerships: Ownership[];
  organisations: Organisation[];
  addBrand = false;
  addModel = false;
  parts: Part[];
  goodIdentificationTypes: GoodIdentificationType[];
  productNumber: ProductNumber;
  originalCategories: ProductCategory[] = [];
  selectedCategories: ProductCategory[] = [];

  private subscription: Subscription;
  private fetcher: Fetcher;
  private refresh$: BehaviorSubject<Date>;

  constructor(
    @Self() public allors: ContextService,
    @Self() public panel: PanelService,
    private metaService: MetaService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    private errorService: ErrorService,
    private stateService: StateService) {

    this.m = this.metaService.m;
    this.refresh$ = new BehaviorSubject(new Date());
    this.fetcher = new Fetcher(this.stateService, this.metaService.pull);

    panel.name = 'detail';
    panel.title = 'Good Details';
    panel.icon = 'person';
    panel.expandable = true;

    // Collapsed
    const pullName = `${this.panel.name}_${this.m.Good.name}`;

    panel.onPull = (pulls) => {

      this.good = undefined;

      if (this.panel.isCollapsed) {
        const { pull, x, m } = this.metaService;
        const id = this.panel.manager.id;

        pulls.push(
          pull.Good({
            name: pullName,
            object: id,
            include: {
              GoodIdentifications: {
                GoodIdentificationType: x
              },
              Part: {
                Brand: x,
                Model: x
              }
            }
          }),
          pull.ProductCategory({ sort: new Sort(m.ProductCategory.Name) }),
        );
      }
    };

    panel.onPulled = (loaded) => {
      if (this.panel.isCollapsed) {
        this.good = loaded.objects[pullName] as Good;
      }
    };
  }

  public ngOnInit(): void {

    // Maximized
    this.subscription = combineLatest(this.refresh$, this.panel.manager.on$, this.stateService.internalOrganisationId$)
      .pipe(
        filter(() => {
          return this.panel.isExpanded;
        }),
        switchMap(([]) => {

          this.good = undefined;

          const { m, pull, x } = this.metaService;
          const id = this.panel.manager.id;

          const pulls = [
            this.fetcher.locales,
            this.fetcher.internalOrganisation,
            pull.VatRate(),
            pull.GoodIdentificationType(),
            pull.ProductCategory({ sort: new Sort(m.ProductCategory.Name) }),
            pull.Part({
              include: {
                Brand: x,
                Model: x
              },
              sort: new Sort(m.Part.Name),
            }),
            pull.Good({
              object: id,
              include: {
                Part: {
                  Brand: x,
                  Model: x
                },
                PrimaryPhoto: x,
                GoodIdentifications: x,
                Photos: x,
                ElectronicDocuments: x,
                LocalisedNames: {
                  Locale: x,
                },
                LocalisedDescriptions: {
                  Locale: x,
                },
                LocalisedComments: {
                  Locale: x,
                },
              },
            }),
            pull.Good({
              name: 'OriginalCategories',
              object: id,
              fetch: { ProductCategoriesWhereProduct : x}
            }),
          ];

          return this.allors.context.load('Pull', new PullRequest({ pulls }));
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        const internalOrganisation = loaded.objects.InternalOrganisation as Organisation;
        this.facility = internalOrganisation.DefaultFacility;

        this.good = loaded.objects.Good as Good;
        this.originalCategories = loaded.collections.OriginalCategories as ProductCategory[];
        this.selectedCategories = this.originalCategories;

        this.categories = loaded.collections.ProductCategories as ProductCategory[];
        this.parts = loaded.collections.Parts as Part[];
        this.vatRates = loaded.collections.VatRates as VatRate[];
        this.goodIdentificationTypes = loaded.collections.GoodIdentificationTypes as GoodIdentificationType[];
        this.locales = loaded.collections.AdditionalLocales as Locale[];

        const goodNumberType = this.goodIdentificationTypes.find((v) => v.UniqueId === 'b640630d-a556-4526-a2e5-60a84ab0db3f');

        this.productNumber = this.good.GoodIdentifications.find(v => v.GoodIdentificationType === goodNumberType);

      }, this.errorService.handler);

  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {

    this.selectedCategories.forEach((category: ProductCategory) => {
      category.AddProduct(this.good);

      const index = this.originalCategories.indexOf(category);
      if (index > -1) {
        this.originalCategories.splice(index, 1);
      }
    });

    this.originalCategories.forEach((category: ProductCategory) => {
      category.RemoveProduct(this.good);
    });

    this.allors.context.save()
      .subscribe(() => {
        this.panel.toggle();
      },
        (error: Error) => {
          this.errorService.handle(error);
        });
  }

  public setDirty(): void {
    this.allors.context.session.hasChanges = true;
  }
}
