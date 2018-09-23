import { AfterViewInit, ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { ActivatedRoute, UrlSegment } from '@angular/router';

import { BehaviorSubject, Observable, Subscription, combineLatest } from 'rxjs';

import { ErrorService, Loaded, Saved, Scope, WorkspaceService, DataService, x } from '../../../../../angular';
import { CatScope, InternalOrganisation, Locale, ProductCategory, Singleton } from '../../../../../domain';
import { Equals, Fetch, PullRequest, Sort, TreeNode } from '../../../../../framework';
import { MetaDomain } from '../../../../../meta';
import { StateService } from '../../../services/StateService';
import { Fetcher } from '../../Fetcher';
import { AllorsMaterialDialogService } from '../../../../base/services/dialog';
import { switchMap } from 'rxjs/operators';
import { LocalisedText } from '../../../../../domain/generated/LocalisedText.g';

@Component({
  templateUrl: './category.component.html',
})
export class CategoryComponent implements OnInit, OnDestroy {

  public m: MetaDomain;

  public category: ProductCategory;
  public title: string;
  public subTitle: string;

  public locales: Locale[];
  public categories: ProductCategory[];
  public catScopes: CatScope[];
  public internalOrganisation: InternalOrganisation;

  private subscription: Subscription;
  private scope: Scope;
  private refresh$: BehaviorSubject<Date>;

  private fetcher: Fetcher;

  constructor(
    private workspaceService: WorkspaceService,
    private dataService: DataService,
    private errorService: ErrorService,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar,
    private dialogService: AllorsMaterialDialogService,
    private stateService: StateService) {

    this.scope = this.workspaceService.createScope();
    this.m = this.workspaceService.metaPopulation.metaDomain;
    this.refresh$ = new BehaviorSubject<Date>(undefined);
    this.fetcher = new Fetcher(this.stateService, this.dataService.pull);
  }

  public ngOnInit(): void {

    const { m, pull } = this.dataService;

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

          return this.scope
            .load('Pull', new PullRequest({ pulls }));
        })
      )
      .subscribe((loaded) => {

        this.internalOrganisation = loaded.objects.internalOrganisation as InternalOrganisation;
        this.category = loaded.objects.category as ProductCategory;
        this.categories = loaded.collections.categories as ProductCategory[];
        this.catScopes = loaded.collections.CatScopes as CatScope[];
        this.locales = loaded.collections.locales as Locale[];

        if (!this.category) {
          this.category = this.scope.session.create('ProductCategory') as ProductCategory;
          this.category.InternalOrganisation = this.internalOrganisation;
        }

        this.title = 'Category';
        this.subTitle = this.category.Name;
      },
        (error: any) => {
          this.errorService.handle(error);
          this.goBack();
        },
      );
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {

    this.scope
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
