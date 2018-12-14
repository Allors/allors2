import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { ActivatedRoute, UrlSegment } from '@angular/router';

import { BehaviorSubject, Observable, Subscription, combineLatest } from 'rxjs';

import { ErrorService, Loaded, Saved, ContextService, MetaService } from '../../../../../angular';
import { CatScope, InternalOrganisation, Locale, ProductCategory, Singleton } from '../../../../../domain';
import { Equals, Fetch, PullRequest, Sort, TreeNode } from '../../../../../framework';
import { Meta } from '../../../../../meta';
import { StateService } from '../../../services/state';
import { Fetcher } from '../../Fetcher';
import { AllorsMaterialDialogService } from '../../../../base/services/dialog';
import { switchMap } from 'rxjs/operators';
import { LocalisedText } from '../../../../../domain/generated/LocalisedText.g';

@Component({
  templateUrl: './category-edit.component.html',
  providers: [ContextService]
})
export class CategoryComponent implements OnInit, OnDestroy {

  public m: Meta;

  public category: ProductCategory;
  public title: string;
  public subTitle: string;

  public locales: Locale[];
  public categories: ProductCategory[];
  public catScopes: CatScope[];
  public internalOrganisation: InternalOrganisation;

  private subscription: Subscription;
  private refresh$: BehaviorSubject<Date>;

  private fetcher: Fetcher;

  constructor(
    @Self() private allors: ContextService,
    public metaService: MetaService,
    private errorService: ErrorService,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar,
    private dialogService: AllorsMaterialDialogService,
    private stateService: StateService) {

    this.m = this.metaService.m;
    this.refresh$ = new BehaviorSubject<Date>(undefined);
    this.fetcher = new Fetcher(this.stateService, this.metaService.pull);
  }

  public ngOnInit(): void {

    const { m, pull, x } = this.metaService;

    this.subscription = combineLatest(this.route.url, this.refresh$, this.stateService.internalOrganisationId$)
      .pipe(
        switchMap(([urlSegments, date, internalOrganisationId]) => {

          const id: string = this.route.snapshot.paramMap.get('id');

          const pulls = [
            this.fetcher.locales,
            this.fetcher.internalOrganisation,
            pull.ProductCategory(
              {
                object: id,
                include: {
                  LocalisedNames: {
                    Locale: x,
                  },
                  LocalisedDescriptions: {
                    Locale: x,
                  }
                }
              }
            ),
            pull.CatScope(),
            pull.ProductCategory({
              sort: new Sort(m.ProductCategory.Name),
            }),
          ];

          return this.allors.context.load('Pull', new PullRequest({ pulls }));
        })
      )
      .subscribe((loaded) => {

        this.internalOrganisation = loaded.objects.internalOrganisation as InternalOrganisation;
        this.category = loaded.objects.category as ProductCategory;
        this.categories = loaded.collections.categories as ProductCategory[];
        this.catScopes = loaded.collections.CatScopes as CatScope[];
        this.locales = loaded.collections.AdditionalLocales as Locale[];

        if (!this.category) {
          this.category = this.allors.context.create('ProductCategory') as ProductCategory;
          this.category.InternalOrganisation = this.internalOrganisation;
        }

        this.title = 'Category';
        this.subTitle = this.category.Name;
      }, this.errorService.handler);
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {

    this.allors.context
      .save()
      .subscribe((saved: Saved) => {
        this.goBack();
      },
        (error: Error) => {
          this.errorService.handle(error);
        });
  }

  public refresh(): void {
    this.refresh$.next(new Date());
  }

  public goBack(): void {
    window.history.back();
  }
}
