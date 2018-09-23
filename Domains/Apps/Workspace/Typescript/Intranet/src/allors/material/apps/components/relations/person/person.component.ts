import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';

import { BehaviorSubject, Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { ErrorService, Saved, x, Allors } from '../../../../../angular';
import { CustomerRelationship, Employment, Enumeration, InternalOrganisation, Locale, Organisation, OrganisationContactKind, OrganisationContactRelationship, Person, PersonRole } from '../../../../../domain';
import { And, Equals, Exists, Not, PullRequest, Sort } from '../../../../../framework';
import { MetaDomain } from '../../../../../meta';
import { StateService } from '../../../services/StateService';
import { Fetcher } from '../../Fetcher';

@Component({
  templateUrl: './person.component.html',
  providers: [Allors]
})
export class PersonComponent implements OnInit, OnDestroy {

  public readonly m: MetaDomain;

  public readonly title = 'Person';
  public subTitle: string;

  public internalOrganisation: InternalOrganisation;
  public person: Person;
  public organisation: Organisation;
  public organisations: Organisation[];
  public organisationContactRelationship: OrganisationContactRelationship;

  public locales: Locale[];
  public genders: Enumeration[];
  public salutations: Enumeration[];
  public organisationContactKinds: OrganisationContactKind[];

  public roles: PersonRole[];
  public selectableRoles: PersonRole[] = [];
  public activeRoles: PersonRole[] = [];
  public customerRelationship: CustomerRelationship;
  public employment: Employment;

  private customerRole: PersonRole;
  private employeeRole: PersonRole;
  private isActiveCustomer: boolean;
  private isActiveEmployee: boolean;
  private subscription: Subscription;

  private readonly refresh$: BehaviorSubject<Date>;
  private readonly fetcher: Fetcher;

  constructor(
    @Self() public allors: Allors,
    private errorService: ErrorService,
    private route: ActivatedRoute,
    private titleService: Title,
    private location: Location,
    private stateService: StateService) {

    this.m = allors.m;
    this.refresh$ = new BehaviorSubject<Date>(undefined);
    this.fetcher = new Fetcher(this.stateService, allors.pull);
    this.titleService.setTitle(this.title);
  }

  public ngOnInit(): void {

    const { scope, m, pull } = this.allors;

    this.subscription = combineLatest(this.route.url, this.refresh$, this.stateService.internalOrganisationId$)
      .pipe(
        switchMap(([urlSegments, date, internalOrganisationId]) => {

          const id: string = this.route.snapshot.paramMap.get('id');
          const organisationId: string = this.route.snapshot.paramMap.get('organisationId');

          let pulls = [
            this.fetcher.internalOrganisation,
            pull.Locale({
              sort: new Sort(m.Locale.Name)
            }),
            pull.GenderType({
              predicate: new Equals(m.GenderType.IsActive, true),
              sort: new Sort(m.GenderType.Name),
            }),
            pull.Salutation({
              predicate: new Equals(m.Salutation.IsActive, true),
              sort: new Sort(m.Salutation.Name),
            }),
            pull.PersonRole({
              sort: new Sort(m.PersonRole.Name)
            }),
            pull.OrganisationContactKind({
              sort: new Sort(m.OrganisationContactKind.Description),
            })
          ];

          if (id) {
            pulls = pulls.concat([
              pull.Person({
                object: id,
                include: {
                  Picture: x,
                }
              }),
              pull.Person({
                object: id,
                fetch: {
                  OrganisationContactRelationshipsWhereContact: x
                }
              }),
              pull.Organisation({
                object: organisationId,
              }),
              pull.CustomerRelationship({
                predicate: new And({
                  operands: [
                    new Equals(m.CustomerRelationship.Customer, id),
                    new Equals(m.CustomerRelationship.InternalOrganisation, internalOrganisationId),
                    new Not({
                      // TODO: create constructor overload for PropertyType
                      operand: new Exists({ propertyType: m.CustomerRelationship.ThroughDate }),
                    })
                  ]
                }),
              }),
              pull.Employment({
                predicate: new And({
                  operands: [
                    new Equals(m.Employment.Employee, id),
                    new Equals(m.Employment.Employer, internalOrganisationId),
                    new Not({
                      operand: new Exists({ propertyType: m.Employment.ThroughDate })
                    })
                  ]
                }),
              })
            ]);

          } else {
            pulls = pulls.concat([
              pull.Organisation({
                sort: new Sort(m.Organisation.PartyName)
              })
            ]);
          }

          return scope
            .load('Pull', new PullRequest({ pulls }));
        })
      )
      .subscribe((loaded) => {
        scope.session.reset();

        this.person = loaded.objects.Person as Person;
        this.organisation = loaded.objects.Organisation as Organisation;
        this.organisations = loaded.collections.Organisations as Organisation[];
        this.internalOrganisation = loaded.objects.InternalOrganisation as InternalOrganisation;
        this.locales = loaded.collections.Locales as Locale[];
        this.genders = loaded.collections.GenderTypes as Enumeration[];
        this.salutations = loaded.collections.Salutations as Enumeration[];
        this.roles = loaded.collections.PersonRoles as PersonRole[];
        this.organisationContactKinds = loaded.collections.OrganisationContactKinds as OrganisationContactKind[];

        if (this.person) {
          this.subTitle = 'edit person';
          [this.customerRelationship] = loaded.collections.CustomerRelationships as CustomerRelationship[];
          [this.employment] = loaded.collections.Employments as Employment[];
          [this.organisationContactRelationship] = loaded.collections.OrganisationContactRelationships as OrganisationContactRelationship[];
          this.organisation = this.organisationContactRelationship.Organisation;

          this.selectableRoles = [];
          this.customerRole = this.roles.find((v: PersonRole) => v.UniqueId.toUpperCase() === 'B29444EF-0950-4D6F-AB3E-9C6DC44C050F');
          this.employeeRole = this.roles.find((v: PersonRole) => v.UniqueId.toUpperCase() === 'DB06A3E1-6146-4C18-A60D-DD10E19F7243');
          this.selectableRoles.push(this.customerRole);
          this.selectableRoles.push(this.employeeRole);

          this.activeRoles = [];
          if (this.internalOrganisation.ActiveCustomers.includes(this.person)) {
            this.isActiveCustomer = true;
            this.activeRoles.push(this.customerRole);
          }

          if (this.internalOrganisation.ActiveEmployees.includes(this.person)) {
            this.isActiveEmployee = true;
            this.activeRoles.push(this.employeeRole);
          }

        } else {
          this.subTitle = 'add a new person';
          this.person = scope.session.create('Person') as Person;
        }

      },
        (error: any) => {
          this.errorService.handle(error);
          this.location.back();
        },
      );
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {

    const { scope } = this.allors;

    if (this.activeRoles.indexOf(this.customerRole) > -1 && !this.isActiveCustomer) {
      const customerRelationship = scope.session.create('CustomerRelationship') as CustomerRelationship;
      customerRelationship.Customer = this.person;
      customerRelationship.InternalOrganisation = this.internalOrganisation;
    }

    if (this.activeRoles.indexOf(this.customerRole) > -1 && this.customerRelationship) {
      this.customerRelationship.ThroughDate = null;
    }

    if (this.activeRoles.indexOf(this.customerRole) === -1 && this.isActiveCustomer) {
      this.customerRelationship.ThroughDate = new Date();
    }

    if (this.activeRoles.indexOf(this.employeeRole) > -1 && !this.isActiveEmployee) {
      const employment = scope.session.create('Employment') as Employment;
      employment.Employee = this.person;
      employment.Employer = this.internalOrganisation;
    }

    if (this.activeRoles.indexOf(this.employeeRole) > -1 && this.employment) {
      this.employment.ThroughDate = null;
    }

    if (this.activeRoles.indexOf(this.employeeRole) === -1 && this.isActiveEmployee) {
      this.employment.ThroughDate = new Date();
    }

    if (this.organisationContactRelationship === undefined && this.organisation !== undefined) {
      const organisationContactRelationship = scope.session.create('OrganisationContactRelationship') as OrganisationContactRelationship;
      organisationContactRelationship.Contact = this.person;
      organisationContactRelationship.Organisation = this.organisation;
    }

    scope
      .save()
      .subscribe((saved: Saved) => {
        this.location.back();
      },
        (error: Error) => {
          this.errorService.handle(error);
        });
  }
}
