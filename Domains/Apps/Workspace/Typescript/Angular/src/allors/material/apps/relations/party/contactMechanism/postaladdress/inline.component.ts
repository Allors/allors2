import { AfterViewInit, ChangeDetectorRef, Component, EventEmitter, OnDestroy, OnInit , Output } from "@angular/core";
import { Validators } from "@angular/forms";
import { MatSnackBar, MatSnackBarConfig } from "@angular/material";
import { ActivatedRoute } from "@angular/router";
import { TdMediaService } from "@covalent/core";
import { Observable, Subject, Subscription } from "rxjs/Rx";

import { AllorsService, ErrorService, Loaded, Saved, Scope } from "../../../../../../angular";
import { Equals, Fetch, Like, Page, Path, PullRequest, PushResponse, Query, Sort, TreeNode } from "../../../../../../domain";
import { ContactMechanismPurpose, Country, Enumeration, PartyContactMechanism, PostalAddress, PostalBoundary } from "../../../../../../domain";
import { MetaDomain } from "../../../../../../meta/index";

@Component({
  selector: "party-contactmechanism-postaladdress",
  template:
  `
  <a-mat-select [object]="partyContactMechanism" [roleType]="m.PartyContactMechanism.ContactPurposes" [options]="contactMechanismPurposes" display="Name"></a-mat-select>
  <a-mat-input  [object]="postalAddress" [roleType]="m.PostalAddress.Address1" label="Address line 1"></a-mat-input>
  <a-mat-input  [object]="postalAddress" [roleType]="m.PostalAddress.Address2" label="Address line 2"></a-mat-input>
  <a-mat-input  [object]="postalAddress" [roleType]="m.PostalAddress.Address3" label="Address line 3"></a-mat-input>
  <a-mat-input  [object]="postalAddress?.PostalBoundary" [roleType]="m.PostalBoundary?.Locality" label="City"></a-mat-input>
  <a-mat-input  [object]="postalAddress?.PostalBoundary" [roleType]="m.PostalBoundary?.PostalCode" label="Postal code"></a-mat-input>
  <a-mat-select [object]="postalAddress?.PostalBoundary" [roleType]="m.PostalBoundary?.Country" [options]="countries" display="Name"></a-mat-select>
  <a-mat-slide-toggle [object]="partyContactMechanism" [roleType]="m.PartyContactMechanism.UseAsDefault" label="Use as default"></a-mat-slide-toggle>
  <a-mat-slide-toggle [object]="partyContactMechanism" [roleType]="m.PartyContactMechanism.NonSolicitationIndicator" label="Non Solicitation"></a-mat-slide-toggle>

  <button mat-button color="primary" type="button" (click)="save()">Save</button>
  <button mat-button color="secondary" type="button"(click)="cancel()">Cancel</button>
`,
})
export class PartyContactMechanismInlinePostalAddressComponent implements OnInit {

  @Output()
  public saved: EventEmitter<string> = new EventEmitter<string>();

  @Output()
  public cancelled: EventEmitter<any> = new EventEmitter();

  public partyContactMechanism: PartyContactMechanism;
  public postalAddress: PostalAddress;
  public postalBoundary: PostalBoundary;
  public countries: Country[];
  public contactMechanismPurposes: ContactMechanismPurpose[];

  public m: MetaDomain;

  private scope: Scope;

  constructor(private allors: AllorsService, private errorService: ErrorService) {

    this.scope = new Scope(allors.database, allors.workspace);
    this.m = this.allors.meta;
  }

  public ngOnInit(): void {
    const query: Query[] = [
      new Query(
        {
          name: "countries",
          objectType: this.m.Country,
        }),
      new Query(
        {
          name: "contactMechanismPurposes",
          objectType: this.m.ContactMechanismPurpose,
        }),
      ];

    this.scope
      .load("Pull", new PullRequest({ query }))
      .subscribe((loaded: Loaded) => {
        this.countries = loaded.collections.countries as Country[];
        this.contactMechanismPurposes = loaded.collections.contactMechanismPurposes as ContactMechanismPurpose[];

        this.partyContactMechanism = this.scope.session.create("PartyContactMechanism") as PartyContactMechanism;
        this.postalAddress = this.scope.session.create("PostalAddress") as PostalAddress;
        this.postalBoundary = this.scope.session.create("PostalBoundary") as PostalBoundary;
        this.partyContactMechanism.ContactMechanism = this.postalAddress;
        this.postalAddress.PostalBoundary = this.postalBoundary;
      },
      (error: any) => {
        this.cancelled.emit();
      },
    );
  }

  public cancel(): void {
    this.cancelled.emit();
  }

  public save(): void {
    this.scope
      .save()
      .subscribe((saved: Saved) => {
        this.saved.emit(this.partyContactMechanism.id);
      },
      (error: Error) => {
        this.errorService.dialog(error);
      });
  }
}
