// Meta extensions
import '@allors/meta/core';
import '@allors/angular/base';
import '@allors/angular/material/custom';

import { NgModule, APP_INITIALIZER } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule, NoopAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule, Routes } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { DateAdapter, MAT_DATE_FORMATS } from '@angular/material/core';
import { MAT_AUTOCOMPLETE_DEFAULT_OPTIONS } from '@angular/material/autocomplete';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatBadgeModule } from '@angular/material/badge';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatSliderModule } from '@angular/material/slider';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSortModule } from '@angular/material/sort';
import { MatStepperModule } from '@angular/material/stepper';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatToolbarModule } from '@angular/material/toolbar';
import { enGB } from 'date-fns/locale';

import { MetaPopulation } from '@allors/meta/system';
import { data, ids } from '@allors/meta/generated';
import { Workspace } from '@allors/domain/system';

// Allors Angular Services Core
import {
  WorkspaceService,
  DatabaseService,
  DatabaseConfig,
  ContextService,
  AuthenticationService,
  DateService,
  AllorsFocusService,
  RefreshService,
  AllorsBarcodeService,
  NavigationService,
  MediaService,
} from '@allors/angular/services/core';

// Allors Angular Core
import {
  DateConfig,
  MediaConfig,
  AuthenticationConfig,
  AuthenticationInterceptor,
  AllorsFocusDirective,
  AllorsBarcodeDirective,
  AuthenticationServiceCore,
  DateServiceCore,
  MediaServiceCore,
  AllorsBarcodeServiceCore,
  AllorsFocusServiceCore,
  NavigationServiceCore,
  RefreshServiceCore,
} from '@allors/angular/core';

// Allors Angular Material Services Core
import {
  AllorsMaterialDialogService,
  ObjectService,
  SaveService,
  AllorsMaterialSideNavService,
  OBJECT_CREATE_TOKEN,
  OBJECT_EDIT_TOKEN,
} from '@allors/angular/material/services/core';

// Allors Angular Material Core
import {
  AllorsMaterialAssociationAutoCompleteComponent,
  AllorsMaterialDialogComponent,
  AllorsMaterialErrorDialogComponent,
  AllorsMaterialFilterFieldDialogComponent,
  AllorsMaterialFilterFieldSearchComponent,
  AllorsMaterialFilterComponent,
  AllorsMaterialFooterComponent,
  AllorsMaterialFooterSaveCancelComponent,
  AllorsMaterialHeaderComponent,
  AllorsMaterialLauncherComponent,
  AllorsMaterialMediaComponent,
  AllorMediaPreviewComponent,
  AllorsMaterialAutocompleteComponent,
  AllorsMaterialCheckboxComponent,
  AllorsMaterialChipsComponent,
  AllorsMaterialDatepickerComponent,
  AllorsMaterialDatetimepickerComponent,
  AllorsMaterialFileComponent,
  AllorsMaterialFilesComponent,
  AllorsMaterialInputComponent,
  AllorsMaterialLocalisedMarkdownComponent,
  AllorsMaterialLocalisedTextComponent,
  AllorsMaterialMarkdownComponent,
  AllorsMaterialMonthpickerComponent,
  AllorsMaterialRadioGroupComponent,
  AllorsMaterialSelectComponent,
  AllorsMaterialSliderComponent,
  AllorsMaterialSlideToggleComponent,
  AllorsMaterialStaticComponent,
  AllorsMaterialTextareaComponent,
  AllorsMaterialScannerComponent,
  AllorsMaterialSideMenuComponent,
  AllorsMaterialSideNavToggleComponent,
  AllorsMaterialTableComponent,
  FactoryFabComponent,
  AllorsDateAdapter,
  AllorsMaterialDialogServiceCore,
  ObjectServiceCore,
  SaveServiceCore,
  AllorsMaterialSideNavServiceCore,
} from '@allors/angular/material/core';

// Angular Material Base
import {
  BasepriceEditComponent,
  BrandEditComponent,
  InlineBrandComponent,
  BrandsOverviewComponent,
  CarrierEditComponent,
  CarrierListComponent,
  CatalogueEditComponent,
  CataloguesListComponent,
  CommunicationEventListComponent,
  CommunicationEventOverviewPanelComponent,
  CommunicationEventWorkTaskComponent,
  ContactMechanismInlineComponent,
  ContactMechanismOverviewPanelComponent,
  CustomerRelationshipEditComponent,
  CustomerShipmentCreateComponent,
  CustomerShipmentOverviewComponent,
  CustomerShipmentOverviewDetailComponent,
  CustomerShipmentOverviewSummaryComponent,
  DisbursementEditComponent,
  EmailAddressCreateComponent,
  EmailAddressEditComponent,
  PartyContactMechanismEmailAddressInlineComponent,
  EmailCommunicationEditComponent,
  EmploymentEditComponent,
  FaceToFaceCommunicationEditComponent,
  FacilityInlineComponent,
  GoodListComponent,
  SelectInternalOrganisationComponent,
  InventoryItemTransactionEditComponent,
  LetterCorrespondenceEditComponent,
  InlineModelComponent,
  NonSerialisedInventoryItemEditComponent,
  NonSerialisedInventoryItemComponent,
  NonUnifiedGoodCreateComponent,
  NonUnifiedGoodOverviewComponent,
  NonUnifiedGoodOverviewDetailComponent,
  NonUnifiedGoodOverviewSummaryComponent,
  NonUnifiedPartCreateComponent,
  NonUnifiedPartListComponent,
  NonUnifiedPartOverviewComponent,
  NonUnifiedPartOverviewDetailComponent,
  NonUnifiedPartOverviewSummaryComponent,
  NotificationLinkComponent,
  NotificationListComponent,
  OrderAdjustmentEditComponent,
  OrderAdjustmentOverviewPanelComponent,
  OrganisationCreateComponent,
  OrganisationInlineComponent,
  OrganisationListComponent,
  OrganisationOverviewComponent,
  OrganisationOverviewDetailComponent,
  OrganisationOverviewSummaryComponent,
  OrganisationContactRelationshipEditComponent,
  PartListComponent,
  PartCategoryEditComponent,
  PartCategoryListComponent,
  PartyInlineComponent,
  PartyContactmechanismEditComponent,
  PartyContactMechanismOverviewPanelComponent,
  PartyRateEditComponent,
  PartyRateOverviewPanelComponent,
  PartyRelationshipOverviewPanelComponent,
  PaymentOverviewPanelComponent,
  PersonCreateComponent,
  PersonInlineComponent,
  PersonListComponent,
  PersonOverviewComponent,
  PersonOverviewDetailComponent,
  PersonOverviewSummaryComponent,
  PhoneCommunicationEditComponent,
  PositionTypeEditComponent,
  PositionTypesOverviewComponent,
  PositionTypeRateEditComponent,
  PositionTypeRatesOverviewComponent,
  PostalAddressCreateComponent,
  PostalAddressEditComponent,
  PartyContactMechanismPostalAddressInlineComponent,
  PriceComponentOverviewPanelComponent,
  ProductCategoryEditComponent,
  ProductCategoryListComponent,
  ProductIdentificationEditComponent,
  ProductIdentificationsPanelComponent,
  ProductQuoteCreateComponent,
  ProductQuoteListComponent,
  ProductQuoteOverviewComponent,
  ProductQuoteOverviewDetailComponent,
  ProductQuoteOverviewPanelComponent,
  ProductQuoteOverviewSummaryComponent,
  ProductQuoteApprovalEditComponent,
  ProductTypeEditComponent,
  ProductTypesOverviewComponent,
  PurchaseInvoiceCreateComponent,
  PurchaseInvoiceListComponent,
  PurchaseInvoiceOverviewComponent,
  PurchaseInvoiceOverviewDetailComponent,
  PurchasInvoiceOverviewSummaryComponent,
  PurchaseInvoiceApprovalEditComponent,
  PurchaseInvoiceItemEditComponent,
  PurchaseInvoiceItemOverviewPanelComponent,
  PurchaseOrderCreateComponent,
  PurchaseOrderListComponent,
  PurchaseOrderOverviewComponent,
  PurchaseOrderOverviewDetailComponent,
  PurchaseOrderInvoiceOverviewPanelComponent,
  PurchaseOrderOverviewPanelComponent,
  PurchaseOrderOverviewSummaryComponent,
  PurchaseOrderApprovalLevel1EditComponent,
  PurchaseOrderApprovalLevel2EditComponent,
  PurchaseOrderItemEditComponent,
  PurchaseOrderItemOverviewPanelComponent,
  PurchaseReturnCreateComponent,
  PurchaseShipmentCreateComponent,
  PurchaseShipmentOverviewComponent,
  PurchaseShipmentOverviewDetailComponent,
  PurchaseShipmentOverviewSummaryComponent,
  QuoteItemEditComponent,
  QuoteItemOverviewPanelComponent,
  ReceiptEditComponent,
  RepeatingPurchaseInvoiceEditComponent,
  RepeatingPurchaseInvoiceOverviewPanelComponent,
  RepeatingSalesInvoiceEditComponent,
  RepeatingSalesInvoiceOverviewPanelComponent,
  RequestForQuoteCreateComponent,
  RequestForQuoteListComponent,
  RequestForQuoteOverviewComponent,
  RequestForQuoteOverviewDetailComponent,
  RequestForQuoteOverviewPanelComponent,
  RequestForQuoteOverviewSummaryComponent,
  RequestItemEditComponent,
  RequestItemOverviewPanelComponent,
  SalesInvoiceCreateComponent,
  SalesInvoiceListComponent,
  SalesInvoiceOverviewComponent,
  SalesInvoiceOverviewDetailComponent,
  SalesInvoiceOverviewPanelComponent,
  SalesInvoiceOverviewSummaryComponent,
  SalesInvoiceItemEditComponent,
  SalesInvoiceItemOverviewPanelComponent,
  SalesOrderCreateComponent,
  SalesOrderListComponent,
  SalesOrderOverviewComponent,
  SalesOrderOverviewDetailComponent,
  SalesOrderOverviewPanelComponent,
  SalesOrderOverviewSummaryComponent,
  SalesOrderItemEditComponent,
  SalesOrderItemOverviewPanelComponent,
  SalesOrderTransferEditComponent,
  SalesOrderTransferOverviewPanelComponent,
  SalesTermEditComponent,
  SalesTermOverviewPanelComponent,
  SerialisedInventoryItemComponent,
  SerialisedItemCreateComponent,
  SerialisedItemListComponent,
  SerialisedItemOverviewComponent,
  SerialisedItemOverviewDetailComponent,
  SerialisedItemOverviewPanelComponent,
  SerialisedItemOverviewSummaryComponent,
  SerialisedItemCharacteristicEditComponent,
  SerialisedItemCharacteristicListComponent,
  ShipmentListComponent,
  ShipmentItemEditComponent,
  ShipmentItemOverviewPanelComponent,
  SubContractorRelationshipEditComponent,
  SupplierOfferingEditComponent,
  SupplierOfferingOverviewPanelComponent,
  SupplierRelationshipEditComponent,
  TaskAssignmentLinkComponent,
  TaskAssignmentListComponent,
  TelecommunicationsNumberCreateComponent,
  TelecommunicationsNumberEditComponent,
  PartyContactMechanismTelecommunicationsNumberInlineComponent,
  TimeEntryEditComponent,
  TimeEntryOverviewPanelComponent,
  UnifiedGoodCreateComponent,
  UnifiedGoodListComponent,
  UnifiedGoodOverviewComponent,
  UnifiedGoodOverviewDetailComponent,
  UnifiedGoodOverviewSummaryComponent,
  UserProfileEditComponent,
  UserProfileLinkComponent,
  WebAddressCreateComponent,
  WebAddressEditComponent,
  InlineWebAddressComponent,
  WorkEffortListComponent,
  WorkEffortOverviewPanelComponent,
  WorkEffortAssignmentRateEditComponent,
  WorkEffortAssignmentRateOverviewPanelComponent,
  WorkEffortFixedAssetAssignmentEditComponent,
  WorkEffortFAAssignmentOverviewPanelComponent,
  WorkEffortInventoryAssignmentEditComponent,
  WorkEffortInventoryAssignmentOverviewPanelComponent,
  WorkEffortPartyAssignmentEditComponent,
  WorkEffortPartyAssignmentOverviewPanelComponent,
  WorkEffortPurchaseOrderItemAssignmentEditComponent,
  WorkEffortPOIAssignmentOverviewPanelComponent,
  WorkTaskCreateComponent,
  WorkTaskOverviewComponent,
  WorkTaskOverviewDetailComponent,
  WorkTaskOverviewPanelComponent,
  WorkTaskOverviewSummaryComponent,
} from '@allors/angular/material/base';

// Angular Material Custom
import { LoginComponent, MainComponent, DashboardComponent, ErrorComponent, AuthorizationService } from '@allors/angular/material/custom';

import { extend as extendDomain } from '@allors/domain/custom';
import { extend as extendAngular, PrintService, PrintConfig, InternalOrganisationId } from '@allors/angular/base';
import { configure as configureMaterial } from '@allors/angular/material/custom';

import { environment } from '../environments/environment';
import { AppComponent } from './app.component';

export const create = {
  [ids.BasePrice]: BasepriceEditComponent,
  [ids.Carrier]: CarrierEditComponent,
  [ids.Catalogue]: CatalogueEditComponent,
  [ids.CustomerRelationship]: CustomerRelationshipEditComponent,
  [ids.CustomerShipment]: CustomerShipmentCreateComponent,
  [ids.Disbursement]: DisbursementEditComponent,
  [ids.DiscountAdjustment]: OrderAdjustmentEditComponent,
  [ids.EanIdentification]: ProductIdentificationEditComponent,
  [ids.EmailAddress]: EmailAddressCreateComponent,
  [ids.EmailCommunication]: EmailCommunicationEditComponent,
  [ids.Employment]: EmploymentEditComponent,
  [ids.FaceToFaceCommunication]: FaceToFaceCommunicationEditComponent,
  [ids.Fee]: OrderAdjustmentEditComponent,
  [ids.IncoTerm]: SalesTermEditComponent,
  [ids.InventoryItemTransaction]: InventoryItemTransactionEditComponent,
  [ids.InvoiceTerm]: SalesTermEditComponent,
  [ids.IsbnIdentification]: ProductIdentificationEditComponent,
  [ids.LetterCorrespondence]: LetterCorrespondenceEditComponent,
  [ids.ManufacturerIdentification]: ProductIdentificationEditComponent,
  [ids.MiscellaneousCharge]: OrderAdjustmentEditComponent,
  [ids.OrderTerm]: SalesTermEditComponent,
  [ids.Organisation]: OrganisationCreateComponent,
  [ids.OrganisationContactRelationship]: OrganisationContactRelationshipEditComponent,
  [ids.NonSerialisedInventoryItem]: NonSerialisedInventoryItemEditComponent,
  [ids.NonUnifiedGood]: NonUnifiedGoodCreateComponent,
  [ids.NonUnifiedPart]: NonUnifiedPartCreateComponent,
  [ids.PartNumber]: ProductIdentificationEditComponent,
  [ids.PartyContactMechanism]: PartyContactmechanismEditComponent,
  [ids.PartyRate]: PartyRateEditComponent,
  [ids.Person]: PersonCreateComponent,
  [ids.PhoneCommunication]: PhoneCommunicationEditComponent,
  [ids.PositionType]: PositionTypeEditComponent,
  [ids.PositionTypeRate]: PositionTypeRateEditComponent,
  [ids.PostalAddress]: PostalAddressCreateComponent,
  [ids.ProductCategory]: ProductCategoryEditComponent,
  [ids.ProductNumber]: ProductIdentificationEditComponent,
  [ids.ProductQuote]: ProductQuoteCreateComponent,
  [ids.ProductType]: ProductTypeEditComponent,
  [ids.PurchaseInvoice]: PurchaseInvoiceCreateComponent,
  [ids.PurchaseInvoiceItem]: PurchaseInvoiceItemEditComponent,
  [ids.PurchaseOrder]: PurchaseOrderCreateComponent,
  [ids.PurchaseOrderItem]: PurchaseOrderItemEditComponent,
  [ids.PurchaseReturn]: PurchaseReturnCreateComponent,
  [ids.PurchaseShipment]: PurchaseShipmentCreateComponent,
  [ids.QuoteItem]: QuoteItemEditComponent,
  [ids.Receipt]: ReceiptEditComponent,
  [ids.RepeatingPurchaseInvoice]: RepeatingPurchaseInvoiceEditComponent,
  [ids.RepeatingSalesInvoice]: RepeatingSalesInvoiceEditComponent,
  [ids.RequestItem]: RequestItemEditComponent,
  [ids.RequestForQuote]: RequestForQuoteCreateComponent,
  [ids.SalesInvoice]: SalesInvoiceCreateComponent,
  [ids.SalesInvoiceItem]: SalesInvoiceItemEditComponent,
  [ids.SalesOrder]: SalesOrderCreateComponent,
  [ids.SalesOrderItem]: SalesOrderItemEditComponent,
  [ids.SerialisedItem]: SerialisedItemCreateComponent,
  [ids.SerialisedItemCharacteristicType]: SerialisedItemCharacteristicEditComponent,
  [ids.ShipmentItem]: ShipmentItemEditComponent,
  [ids.ShippingAndHandlingCharge]: OrderAdjustmentEditComponent,
  [ids.SkuIdentification]: ProductIdentificationEditComponent,
  [ids.SubContractorRelationship]: SubContractorRelationshipEditComponent,
  [ids.SupplierOffering]: SupplierOfferingEditComponent,
  [ids.SupplierRelationship]: SupplierRelationshipEditComponent,
  [ids.SurchargeAdjustment]: OrderAdjustmentEditComponent,
  [ids.TelecommunicationsNumber]: TelecommunicationsNumberCreateComponent,
  [ids.TimeEntry]: TimeEntryEditComponent,
  [ids.UnifiedGood]: UnifiedGoodCreateComponent,
  [ids.UpcaIdentification]: ProductIdentificationEditComponent,
  [ids.UpceIdentification]: ProductIdentificationEditComponent,
  [ids.WebAddress]: WebAddressCreateComponent,
  [ids.WorkEffortAssignmentRate]: WorkEffortAssignmentRateEditComponent,
  [ids.WorkEffortFixedAssetAssignment]: WorkEffortFixedAssetAssignmentEditComponent,
  [ids.WorkEffortInventoryAssignment]: WorkEffortInventoryAssignmentEditComponent,
  [ids.WorkEffortPartyAssignment]: WorkEffortPartyAssignmentEditComponent,
  // [ids.WorkEffortPurchaseOrderItemAssignment]: WorkEffortPurchaseOrderItemAssignmentEditComponent,
  [ids.WorkTask]: WorkTaskCreateComponent,
};

export const edit = {
  [ids.BasePrice]: BasepriceEditComponent,
  [ids.Carrier]: CarrierEditComponent,
  [ids.Catalogue]: CatalogueEditComponent,
  [ids.CustomerRelationship]: CustomerRelationshipEditComponent,
  [ids.Disbursement]: DisbursementEditComponent,
  [ids.DiscountAdjustment]: OrderAdjustmentEditComponent,
  [ids.EanIdentification]: ProductIdentificationEditComponent,
  [ids.EmailAddress]: EmailAddressEditComponent,
  [ids.EmailCommunication]: EmailCommunicationEditComponent,
  [ids.Employment]: EmploymentEditComponent,
  [ids.FaceToFaceCommunication]: FaceToFaceCommunicationEditComponent,
  [ids.Fee]: OrderAdjustmentEditComponent,
  [ids.IncoTerm]: SalesTermEditComponent,
  [ids.InventoryItemTransaction]: InventoryItemTransactionEditComponent,
  [ids.InvoiceTerm]: SalesTermEditComponent,
  [ids.IsbnIdentification]: ProductIdentificationEditComponent,
  [ids.LetterCorrespondence]: LetterCorrespondenceEditComponent,
  [ids.ManufacturerIdentification]: ProductIdentificationEditComponent,
  [ids.MiscellaneousCharge]: OrderAdjustmentEditComponent,
  [ids.NonSerialisedInventoryItem]: NonSerialisedInventoryItemEditComponent,
  [ids.OrderTerm]: SalesTermEditComponent,
  [ids.OrganisationContactRelationship]: OrganisationContactRelationshipEditComponent,
  [ids.PartyContactMechanism]: PartyContactmechanismEditComponent,
  [ids.PartyRate]: PartyRateEditComponent,
  [ids.PhoneCommunication]: PhoneCommunicationEditComponent,
  [ids.PositionType]: PositionTypeEditComponent,
  [ids.PositionTypeRate]: PositionTypeRateEditComponent,
  [ids.PostalAddress]: PostalAddressEditComponent,
  [ids.ProductCategory]: ProductCategoryEditComponent,
  [ids.PartNumber]: ProductIdentificationEditComponent,
  [ids.ProductNumber]: ProductIdentificationEditComponent,
  [ids.ProductType]: ProductTypeEditComponent,
  [ids.ProductQuoteApproval]: ProductQuoteApprovalEditComponent,
  [ids.PurchaseInvoiceApproval]: PurchaseInvoiceApprovalEditComponent,
  [ids.PurchaseInvoiceItem]: PurchaseInvoiceItemEditComponent,
  [ids.PurchaseOrderApprovalLevel1]: PurchaseOrderApprovalLevel1EditComponent,
  [ids.PurchaseOrderApprovalLevel2]: PurchaseOrderApprovalLevel2EditComponent,
  [ids.PurchaseOrderItem]: PurchaseOrderItemEditComponent,
  [ids.QuoteItem]: QuoteItemEditComponent,
  [ids.Receipt]: ReceiptEditComponent,
  [ids.RepeatingPurchaseInvoice]: RepeatingPurchaseInvoiceEditComponent,
  [ids.RepeatingSalesInvoice]: RepeatingSalesInvoiceEditComponent,
  [ids.RequestItem]: RequestItemEditComponent,
  [ids.SalesInvoiceItem]: SalesInvoiceItemEditComponent,
  [ids.SalesOrderItem]: SalesOrderItemEditComponent,
  [ids.SerialisedItemCharacteristicType]: SerialisedItemCharacteristicEditComponent,
  [ids.ShipmentItem]: ShipmentItemEditComponent,
  [ids.ShippingAndHandlingCharge]: OrderAdjustmentEditComponent,
  [ids.SkuIdentification]: ProductIdentificationEditComponent,
  [ids.SupplierOffering]: SupplierOfferingEditComponent,
  [ids.SubContractorRelationship]: SubContractorRelationshipEditComponent,
  [ids.SupplierRelationship]: SupplierRelationshipEditComponent,
  [ids.SurchargeAdjustment]: OrderAdjustmentEditComponent,
  [ids.TelecommunicationsNumber]: TelecommunicationsNumberEditComponent,
  [ids.TimeEntry]: TimeEntryEditComponent,
  [ids.UpcaIdentification]: ProductIdentificationEditComponent,
  [ids.UpceIdentification]: ProductIdentificationEditComponent,
  [ids.UserProfile]: UserProfileEditComponent,
  [ids.WebAddress]: WebAddressEditComponent,
  [ids.WorkEffortAssignmentRate]: WorkEffortAssignmentRateEditComponent,
  [ids.WorkEffortFixedAssetAssignment]: WorkEffortFixedAssetAssignmentEditComponent,
  [ids.WorkEffortInventoryAssignment]: WorkEffortInventoryAssignmentEditComponent,
  // [ids.WorkEffortPurchaseOrderItemAssignment]: WorkEffortPurchaseOrderItemAssignmentEditComponent,
  [ids.WorkEffortPartyAssignment]: WorkEffortPartyAssignmentEditComponent,
};

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'error', component: ErrorComponent },
  {
    canActivate: [AuthorizationService],
    path: '',
    component: MainComponent,
    children: [
      {
        path: '',
        component: DashboardComponent,
        pathMatch: 'full',
      },
      {
        path: 'contacts',
        children: [
          { path: 'people', component: PersonListComponent },
          { path: 'person/:id', component: PersonOverviewComponent },
          { path: 'organisations', component: OrganisationListComponent },
          { path: 'organisation/:id', component: OrganisationOverviewComponent },
          { path: 'communicationevents', component: CommunicationEventListComponent },
        ],
      },

      {
        path: 'sales',
        children: [
          { path: 'requestsforquote', component: RequestForQuoteListComponent },
          { path: 'requestforquote/:id', component: RequestForQuoteOverviewComponent },
          { path: 'productquotes', component: ProductQuoteListComponent },
          { path: 'productquote/:id', component: ProductQuoteOverviewComponent },
          { path: 'salesorders', component: SalesOrderListComponent },
          { path: 'salesorder/:id', component: SalesOrderOverviewComponent },
          { path: 'salesinvoices', component: SalesInvoiceListComponent },
          { path: 'salesinvoice/:id', component: SalesInvoiceOverviewComponent },
        ],
      },

      {
        path: 'products',
        children: [
          { path: 'goods', component: GoodListComponent },
          { path: 'nonunifiedgood/:id', component: NonUnifiedGoodOverviewComponent },
          { path: 'parts', component: PartListComponent },
          { path: 'nonunifiedpart/:id', component: NonUnifiedPartOverviewComponent },
          { path: 'catalogues', component: CataloguesListComponent },
          { path: 'productcategories', component: ProductCategoryListComponent },
          { path: 'serialiseditemcharacteristics', component: SerialisedItemCharacteristicListComponent },
          { path: 'producttypes', component: ProductTypesOverviewComponent },
          { path: 'serialiseditems', component: SerialisedItemListComponent },
          { path: 'serialisedItem/:id', component: SerialisedItemOverviewComponent },
          { path: 'unifiedgoods', component: UnifiedGoodListComponent },
          { path: 'unifiedgood/:id', component: UnifiedGoodOverviewComponent },
        ],
      },

      {
        path: 'purchasing',
        children: [
          { path: 'purchaseorders', component: PurchaseOrderListComponent },
          { path: 'purchaseorder/:id', component: PurchaseOrderOverviewComponent },
          { path: 'purchaseinvoices', component: PurchaseInvoiceListComponent },
          { path: 'purchaseinvoice/:id', component: PurchaseInvoiceOverviewComponent },
        ],
      },

      {
        path: 'shipment',
        children: [
          { path: 'shipments', component: ShipmentListComponent },
          { path: 'customershipment/:id', component: CustomerShipmentOverviewComponent },
          { path: 'purchaseshipment/:id', component: PurchaseShipmentOverviewComponent },
          { path: 'carriers', component: CarrierListComponent },
        ],
      },

      {
        path: 'workefforts',
        children: [
          { path: 'workefforts', component: WorkEffortListComponent },
          { path: 'worktask/:id', component: WorkTaskOverviewComponent },
        ],
      },

      {
        path: 'humanresource',
        children: [
          { path: 'positiontypes', component: PositionTypesOverviewComponent },
          { path: 'positiontyperates', component: PositionTypeRatesOverviewComponent },
        ],
      },

      {
        path: 'workflow',
        children: [{ path: 'taskassignments', component: TaskAssignmentListComponent }],
      },
    ],
  },
];

export function appInitFactory(workspaceService: WorkspaceService, internalOrganisationId: InternalOrganisationId) {
  return () => {
    const metaPopulation = new MetaPopulation(data);
    const workspace = new Workspace(metaPopulation);

    // Domain extensions
    extendDomain(workspace);
    extendAngular(workspace);

    // Configuration
    configureMaterial(metaPopulation, internalOrganisationId);

    workspaceService.workspace = workspace;
  };
}

@NgModule({
  bootstrap: [AppComponent],
  declarations: [
    AllorsFocusDirective,
    AllorsBarcodeDirective,
    // Angular Material Core
    AllorsMaterialAssociationAutoCompleteComponent,
    AllorsMaterialDialogComponent,
    AllorsMaterialErrorDialogComponent,
    AllorsMaterialFilterComponent,
    AllorsMaterialFilterFieldDialogComponent,
    AllorsMaterialFilterFieldSearchComponent,
    AllorsMaterialFooterComponent,
    AllorsMaterialFooterSaveCancelComponent,
    AllorsMaterialHeaderComponent,
    AllorsMaterialLauncherComponent,
    AllorsMaterialMediaComponent,
    AllorMediaPreviewComponent,
    AllorsMaterialAutocompleteComponent,
    AllorsMaterialCheckboxComponent,
    AllorsMaterialChipsComponent,
    AllorsMaterialDatepickerComponent,
    AllorsMaterialDatetimepickerComponent,
    AllorsMaterialFileComponent,
    AllorsMaterialFilesComponent,
    AllorsMaterialInputComponent,
    AllorsMaterialLocalisedMarkdownComponent,
    AllorsMaterialLocalisedTextComponent,
    AllorsMaterialMarkdownComponent,
    AllorsMaterialMonthpickerComponent,
    AllorsMaterialRadioGroupComponent,
    AllorsMaterialSelectComponent,
    AllorsMaterialSliderComponent,
    AllorsMaterialSlideToggleComponent,
    AllorsMaterialStaticComponent,
    AllorsMaterialTextareaComponent,
    AllorsMaterialScannerComponent,
    AllorsMaterialSideMenuComponent,
    AllorsMaterialSideNavToggleComponent,
    AllorsMaterialTableComponent,
    FactoryFabComponent,
    // Angular Material Base
    BasepriceEditComponent,
    BrandEditComponent,
    InlineBrandComponent,
    BrandsOverviewComponent,
    CarrierEditComponent,
    CarrierListComponent,
    CatalogueEditComponent,
    CataloguesListComponent,
    CommunicationEventListComponent,
    CommunicationEventOverviewPanelComponent,
    CommunicationEventWorkTaskComponent,
    ContactMechanismInlineComponent,
    ContactMechanismOverviewPanelComponent,
    CustomerRelationshipEditComponent,
    CustomerShipmentCreateComponent,
    CustomerShipmentOverviewComponent,
    CustomerShipmentOverviewDetailComponent,
    CustomerShipmentOverviewSummaryComponent,
    DisbursementEditComponent,
    EmailAddressCreateComponent,
    EmailAddressEditComponent,
    PartyContactMechanismEmailAddressInlineComponent,
    EmailCommunicationEditComponent,
    EmploymentEditComponent,
    FaceToFaceCommunicationEditComponent,
    FacilityInlineComponent,
    GoodListComponent,
    SelectInternalOrganisationComponent,
    InventoryItemTransactionEditComponent,
    LetterCorrespondenceEditComponent,
    InlineModelComponent,
    NonSerialisedInventoryItemEditComponent,
    NonSerialisedInventoryItemComponent,
    NonUnifiedGoodCreateComponent,
    NonUnifiedGoodOverviewComponent,
    NonUnifiedGoodOverviewDetailComponent,
    NonUnifiedGoodOverviewSummaryComponent,
    NonUnifiedPartCreateComponent,
    NonUnifiedPartListComponent,
    NonUnifiedPartOverviewComponent,
    NonUnifiedPartOverviewDetailComponent,
    NonUnifiedPartOverviewSummaryComponent,
    NotificationLinkComponent,
    NotificationListComponent,
    OrderAdjustmentEditComponent,
    OrderAdjustmentOverviewPanelComponent,
    OrganisationCreateComponent,
    OrganisationInlineComponent,
    OrganisationListComponent,
    OrganisationOverviewComponent,
    OrganisationOverviewDetailComponent,
    OrganisationOverviewSummaryComponent,
    OrganisationContactRelationshipEditComponent,
    PartListComponent,
    PartCategoryEditComponent,
    PartCategoryListComponent,
    PartyInlineComponent,
    PartyContactmechanismEditComponent,
    PartyContactMechanismOverviewPanelComponent,
    PartyRateEditComponent,
    PartyRateOverviewPanelComponent,
    PartyRelationshipOverviewPanelComponent,
    PaymentOverviewPanelComponent,
    PersonCreateComponent,
    PersonInlineComponent,
    PersonListComponent,
    PersonOverviewComponent,
    PersonOverviewDetailComponent,
    PersonOverviewSummaryComponent,
    PhoneCommunicationEditComponent,
    PositionTypeEditComponent,
    PositionTypesOverviewComponent,
    PositionTypeRateEditComponent,
    PositionTypeRatesOverviewComponent,
    PostalAddressCreateComponent,
    PostalAddressEditComponent,
    PartyContactMechanismPostalAddressInlineComponent,
    PriceComponentOverviewPanelComponent,
    ProductCategoryEditComponent,
    ProductCategoryListComponent,
    ProductIdentificationEditComponent,
    ProductIdentificationsPanelComponent,
    ProductQuoteCreateComponent,
    ProductQuoteListComponent,
    ProductQuoteOverviewComponent,
    ProductQuoteOverviewDetailComponent,
    ProductQuoteOverviewPanelComponent,
    ProductQuoteOverviewSummaryComponent,
    ProductQuoteApprovalEditComponent,
    ProductTypeEditComponent,
    ProductTypesOverviewComponent,
    PurchaseInvoiceCreateComponent,
    PurchaseInvoiceListComponent,
    PurchaseInvoiceOverviewComponent,
    PurchaseInvoiceOverviewDetailComponent,
    PurchasInvoiceOverviewSummaryComponent,
    PurchaseInvoiceApprovalEditComponent,
    PurchaseInvoiceItemEditComponent,
    PurchaseInvoiceItemOverviewPanelComponent,
    PurchaseOrderCreateComponent,
    PurchaseOrderListComponent,
    PurchaseOrderOverviewComponent,
    PurchaseOrderOverviewDetailComponent,
    PurchaseOrderInvoiceOverviewPanelComponent,
    PurchaseOrderOverviewPanelComponent,
    PurchaseOrderOverviewSummaryComponent,
    PurchaseOrderApprovalLevel1EditComponent,
    PurchaseOrderApprovalLevel2EditComponent,
    PurchaseOrderItemEditComponent,
    PurchaseOrderItemOverviewPanelComponent,
    PurchaseReturnCreateComponent,
    PurchaseShipmentCreateComponent,
    PurchaseShipmentOverviewComponent,
    PurchaseShipmentOverviewDetailComponent,
    PurchaseShipmentOverviewSummaryComponent,
    QuoteItemEditComponent,
    QuoteItemOverviewPanelComponent,
    ReceiptEditComponent,
    RepeatingPurchaseInvoiceEditComponent,
    RepeatingPurchaseInvoiceOverviewPanelComponent,
    RepeatingSalesInvoiceEditComponent,
    RepeatingSalesInvoiceOverviewPanelComponent,
    RequestForQuoteCreateComponent,
    RequestForQuoteListComponent,
    RequestForQuoteOverviewComponent,
    RequestForQuoteOverviewDetailComponent,
    RequestForQuoteOverviewPanelComponent,
    RequestForQuoteOverviewSummaryComponent,
    RequestItemEditComponent,
    RequestItemOverviewPanelComponent,
    SalesInvoiceCreateComponent,
    SalesInvoiceListComponent,
    SalesInvoiceOverviewComponent,
    SalesInvoiceOverviewDetailComponent,
    SalesInvoiceOverviewPanelComponent,
    SalesInvoiceOverviewSummaryComponent,
    SalesInvoiceItemEditComponent,
    SalesInvoiceItemOverviewPanelComponent,
    SalesOrderCreateComponent,
    SalesOrderListComponent,
    SalesOrderOverviewComponent,
    SalesOrderOverviewDetailComponent,
    SalesOrderOverviewPanelComponent,
    SalesOrderOverviewSummaryComponent,
    SalesOrderItemEditComponent,
    SalesOrderItemOverviewPanelComponent,
    SalesOrderTransferEditComponent,
    SalesOrderTransferOverviewPanelComponent,
    SalesTermEditComponent,
    SalesTermOverviewPanelComponent,
    SerialisedInventoryItemComponent,
    SerialisedItemCreateComponent,
    SerialisedItemListComponent,
    SerialisedItemOverviewComponent,
    SerialisedItemOverviewDetailComponent,
    SerialisedItemOverviewPanelComponent,
    SerialisedItemOverviewSummaryComponent,
    SerialisedItemCharacteristicEditComponent,
    SerialisedItemCharacteristicListComponent,
    ShipmentListComponent,
    ShipmentItemEditComponent,
    ShipmentItemOverviewPanelComponent,
    SubContractorRelationshipEditComponent,
    SupplierOfferingEditComponent,
    SupplierOfferingOverviewPanelComponent,
    SupplierRelationshipEditComponent,
    TaskAssignmentLinkComponent,
    TaskAssignmentListComponent,
    TelecommunicationsNumberCreateComponent,
    TelecommunicationsNumberEditComponent,
    PartyContactMechanismTelecommunicationsNumberInlineComponent,
    TimeEntryEditComponent,
    TimeEntryOverviewPanelComponent,
    UnifiedGoodCreateComponent,
    UnifiedGoodListComponent,
    UnifiedGoodOverviewComponent,
    UnifiedGoodOverviewDetailComponent,
    UnifiedGoodOverviewSummaryComponent,
    UserProfileEditComponent,
    UserProfileLinkComponent,
    WebAddressCreateComponent,
    WebAddressEditComponent,
    InlineWebAddressComponent,
    WorkEffortListComponent,
    WorkEffortOverviewPanelComponent,
    WorkEffortAssignmentRateEditComponent,
    WorkEffortAssignmentRateOverviewPanelComponent,
    WorkEffortFixedAssetAssignmentEditComponent,
    WorkEffortFAAssignmentOverviewPanelComponent,
    WorkEffortInventoryAssignmentEditComponent,
    WorkEffortInventoryAssignmentOverviewPanelComponent,
    WorkEffortPartyAssignmentEditComponent,
    WorkEffortPartyAssignmentOverviewPanelComponent,
    WorkEffortPurchaseOrderItemAssignmentEditComponent,
    WorkEffortPOIAssignmentOverviewPanelComponent,
    WorkTaskCreateComponent,
    WorkTaskOverviewComponent,
    WorkTaskOverviewDetailComponent,
    WorkTaskOverviewPanelComponent,
    WorkTaskOverviewSummaryComponent,
    // Angular Material Custom
    LoginComponent,
    MainComponent,
    DashboardComponent,
    ErrorComponent,
    // App
    AppComponent,
  ],
  imports: [
    BrowserModule,
    environment.production ? BrowserAnimationsModule : NoopAnimationsModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forRoot(routes),

    MatAutocompleteModule,
    MatBadgeModule,
    MatButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatChipsModule,
    MatDatepickerModule,
    MatDialogModule,
    MatFormFieldModule,
    MatGridListModule,
    MatIconModule,
    MatInputModule,
    MatListModule,
    MatMenuModule,
    MatPaginatorModule,
    MatRadioModule,
    MatSelectModule,
    MatSidenavModule,
    MatSliderModule,
    MatSlideToggleModule,
    MatSnackBarModule,
    MatSortModule,
    MatTabsModule,
    MatStepperModule,
    MatTableModule,
    MatToolbarModule,
  ],
  providers: [
    {
      provide: APP_INITIALIZER,
      useFactory: appInitFactory,
      deps: [WorkspaceService, InternalOrganisationId],
      multi: true,
    },
    DatabaseService,
    { provide: DatabaseConfig, useValue: { url: environment.url } },
    WorkspaceService,
    ContextService,
    { provide: AuthenticationService, useClass: AuthenticationServiceCore },
    {
      provide: AuthenticationConfig,
      useValue: {
        url: environment.url + environment.authenticationUrl,
      },
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthenticationInterceptor,
      multi: true,
    },
    { provide: AllorsBarcodeService, useClass: AllorsBarcodeServiceCore },
    { provide: DateService, useClass: DateServiceCore },
    {
      provide: DateConfig,
      useValue: {
        locale: enGB,
      },
    },
    { provide: AllorsFocusService, useClass: AllorsFocusServiceCore },
    { provide: MediaService, useClass: MediaServiceCore },
    { provide: MediaConfig, useValue: { url: environment.url } },
    { provide: NavigationService, useClass: NavigationServiceCore },
    { provide: RefreshService, useClass: RefreshServiceCore },

    // Angular Base
    PrintService,
    { provide: PrintConfig, useValue: { url: environment.url } },


    // Angular Material
    {
      provide: MAT_AUTOCOMPLETE_DEFAULT_OPTIONS,
      useValue: { autoActiveFirstOption: true },
    },
    { provide: DateAdapter, useClass: AllorsDateAdapter },
    {
      provide: MAT_DATE_FORMATS,
      useValue: {
        parse: {
          dateInput: 'dd/MM/yyyy',
        },
        display: {
          dateInput: 'dd/MM/yyyy',
          monthYearLabel: 'LLL y',
          dateA11yLabel: 'MMMM d, y',
          monthYearA11yLabel: 'MMMM y',
        },
      },
    },
    { provide: AllorsMaterialDialogService, useClass: AllorsMaterialDialogServiceCore },
    { provide: ObjectService, useClass: ObjectServiceCore },
    { provide: SaveService, useClass: SaveServiceCore },
    { provide: AllorsMaterialSideNavService, useClass: AllorsMaterialSideNavServiceCore },
    { provide: ObjectService, useClass: ObjectServiceCore },
    { provide: OBJECT_CREATE_TOKEN, useValue: create },
    { provide: OBJECT_EDIT_TOKEN, useValue: edit },
  ],
})
export class AppModule {}
