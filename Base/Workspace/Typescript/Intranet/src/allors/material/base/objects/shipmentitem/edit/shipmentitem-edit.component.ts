import * as moment from 'moment';

import { Component, OnDestroy, OnInit, Self, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

import { Subscription, combineLatest } from 'rxjs';

import { ContextService, MetaService, RefreshService, TestScope } from '../../../../../angular';
import { NonUnifiedGood, InventoryItem, NonSerialisedInventoryItem, Product, Shipment, ShipmentItem, SerialisedInventoryItem, SerialisedItem, Part, OrderShipment, SalesOrderItem, Good, SalesOrderItemState, SalesOrderState, SalesOrderItemShipmentState, SerialisedItemState, RequestItemState, RequestState, QuoteItemState, QuoteState, ShipmentItemState, ShipmentState, PurchaseOrderItem, PurchaseOrderState, NonUnifiedPart, SupplierOffering } from '../../../../../domain';
import { PullRequest, IObject, Equals, Sort } from '../../../../../framework';
import { Meta } from '../../../../../meta';
import { switchMap, map } from 'rxjs/operators';
import { ObjectData, SaveService, FiltersService } from '../../../../../material';
import { ShipmentReceipt } from 'src/allors/domain/generated/ShipmentReceipt.g';

@Component({
  templateUrl: './shipmentitem-edit.component.html',
  providers: [ContextService]
})
export class ShipmentItemEditComponent extends TestScope implements OnInit, OnDestroy {

  readonly m: Meta;

  title: string;
  shipment: Shipment;
  shipmentItem: ShipmentItem;
  inventoryItems: InventoryItem[];
  serialisedInventoryItem: SerialisedInventoryItem;
  nonSerialisedInventoryItem: NonSerialisedInventoryItem;
  serialisedItems: SerialisedItem[] = [];
  serialisedItemStates: SerialisedItemState[];
  soldState: SerialisedItemState;
  orderShipment: OrderShipment;
  shipmentReceipt: ShipmentReceipt;
  salesOrderItems: SalesOrderItem[] = [];
  selectedSalesOrderItem: SalesOrderItem;
  selectedPurchaseOrderItem: PurchaseOrderItem;
  purchaseOrderItems: PurchaseOrderItem[] = [];
  parts: Part[];
  supplierOfferings: SupplierOffering[];

  draftRequestItem: RequestItemState;
  submittedRequestItem: RequestItemState;
  anonymousRequest: RequestState;
  submittedRequest: RequestState;
  pendingCustomerRequest: RequestState;
  draftQuoteItem: QuoteItemState;
  submittedQuoteItem: QuoteItemState;
  approvedQuoteItem: QuoteItemState;
  createdQuote: QuoteState;
  approvedQuote: QuoteState;
  provisionalOrderItem: SalesOrderItemState;
  requestsApprovalOrderItem: SalesOrderItemState;
  onHoldOrderItem: SalesOrderItemState;
  inProcessOrderItem: SalesOrderItemState;
  provisionalOrder: SalesOrderState;
  requestsApprovalOrder: SalesOrderState;
  onHoldOrder: SalesOrderState;
  inProcessOrder: SalesOrderState;
  createdOrderItem: SalesOrderItemState;
  createdShipmentItem: ShipmentItemState;
  pickingShipmentItem: ShipmentItemState;
  pickedShipmentItem: ShipmentItemState;
  packedShipmentItem: ShipmentItemState;
  createdShipment: ShipmentState;
  pickingShipment: ShipmentState;
  pickedShipment: ShipmentState;
  packedShipment: ShipmentState;
  onholdShipment: ShipmentState;
  isCustomerShipment: boolean;
  isPurchaseShipment: boolean;

  private previousGood;
  private previousPart;
  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    @Inject(MAT_DIALOG_DATA) public data: ObjectData,
    public filtersService: FiltersService,
    public dialogRef: MatDialogRef<ShipmentItemEditComponent>,
    public metaService: MetaService,
    public refreshService: RefreshService,
    private saveService: SaveService,
    public snackBar: MatSnackBar
  ) {
    super();

    this.m = this.metaService.m;
  }

  public ngOnInit(): void {

    const { pull, x, m } = this.metaService;

    this.subscription = combineLatest(this.refreshService.refresh$)
      .pipe(
        switchMap(() => {

          const isCreate = this.data.id === undefined;

          const pulls = [
            pull.ShipmentItem({
              object: this.data.id,
              include: {
                SyncedShipment: x,
                Good: x,
                Part: x,
                SerialisedItem: x,
                ShipmentItemState: x
              }
            }),
            pull.ShipmentItem({
              object: this.data.id,
              fetch: {
                OrderShipmentsWhereShipmentItem: {
                  include: {
                    OrderItem: x
                  }
                }
              }
            }),
            pull.Shipment({
              object: this.data.associationId,
              include: {
                ShipToAddress: x
              }
            }),
            pull.Shipment({
              object: this.data.associationId,
              fetch: {
                ShipToAddress: {
                  SalesOrderItemsWhereShipToAddress: {
                    include: {
                      Product: x,
                      SerialisedItem: x,
                      SalesOrderWhereSalesOrderItem: {
                        SalesOrderState: x
                      }
                    }
                  }
                }
              }
            }),
            pull.Shipment({
              object: this.data.associationId,
              fetch: {
                ShipFromParty: {
                  PurchaseOrdersWhereTakenViaSupplier: {
                    include: {
                      PurchaseOrderItems: {
                        Part: x,
                        SerialisedItem: x,
                        PurchaseOrderItemState: x,
                        PurchaseOrderWherePurchaseOrderItem: {
                          PurchaseOrderState: x
                        }
                      }
                    }
                  }
                }
              }
            }),
            pull.Shipment({
              object: this.data.associationId,
              fetch: {
                ShipFromParty: {
                  SupplierOfferingsWhereSupplier: {
                    include: {
                      Part: x,
                    }
                  }
                }
              }
            }),
            pull.SerialisedItemState(),
            pull.SerialisedInventoryItemState(
              {
                predicate: new Equals({ propertyType: m.SerialisedInventoryItemState.IsActive, value: true }),
                sort: new Sort(m.SerialisedInventoryItemState.Name),
              }
            ),
            pull.RequestItemState(),
            pull.RequestState(),
            pull.QuoteItemState(),
            pull.QuoteState(),
            pull.SalesOrderItemState(),
            pull.SalesOrderState(),
            pull.PurchaseOrderItemState(),
            pull.PurchaseOrderState(),
            pull.ShipmentItemState(),
            pull.ShipmentState(),
          ];

          return this.allors.context
            .load(new PullRequest({ pulls }))
            .pipe(
              map((loaded) => ({ loaded, isCreate }))
            );
        })
      )
      .subscribe(({ loaded, isCreate }) => {
        this.allors.context.reset();

        const now = moment.utc();

        this.shipmentItem = loaded.objects.ShipmentItem as ShipmentItem;
        this.shipment = loaded.objects.Shipment as Shipment || this.shipmentItem.SyncedShipment;
        this.isCustomerShipment = this.shipment.objectType === this.metaService.m.CustomerShipment;
        this.isPurchaseShipment = this.shipment.objectType === this.metaService.m.PurchaseShipment;

        this.serialisedItemStates = loaded.collections.SerialisedItemStates as SerialisedItemState[];
        this.soldState = this.serialisedItemStates.find((v: SerialisedItemState) => v.UniqueId === 'feccf869-98d7-4e9c-8979-5611a43918bc');

        const salesOrderStates = loaded.collections.SalesOrderStates as SalesOrderState[];
        const inProcess = salesOrderStates.find((v) => v.UniqueId === 'ddbb678e-9a66-4842-87fd-4e628cff0a75');
        this.provisionalOrder = salesOrderStates.find((v: SalesOrderState) => v.UniqueId === '29abc67d-4be1-4af3-b993-64e9e36c3e6b');
        this.requestsApprovalOrder = salesOrderStates.find((v: SalesOrderState) => v.UniqueId === '6b6f6e25-4da1-455d-9c9f-21f2d4316d66');
        this.onHoldOrder = salesOrderStates.find((v: SalesOrderState) => v.UniqueId === 'f625fb7e-893e-4f68-ab7b-2bc29a644e5b');
        this.inProcessOrder = salesOrderStates.find((v: SalesOrderState) => v.UniqueId === 'ddbb678e-9a66-4842-87fd-4e628cff0a75');

        const salesOrderItemStates = loaded.collections.SalesOrderItemStates as SalesOrderItemState[];
        this.createdOrderItem = salesOrderItemStates.find((v: SalesOrderItemState) => v.UniqueId === '5b0993b5-5784-4e8d-b1ad-93affac9a913');
        this.onHoldOrderItem = salesOrderItemStates.find((v: SalesOrderItemState) => v.UniqueId === '3b185d51-af4a-441e-be0d-f91cfcbdb5d8');
        this.inProcessOrderItem = salesOrderItemStates.find((v: SalesOrderItemState) => v.UniqueId === 'e08401f7-1deb-4b27-b0c5-8f034bffedb5');

        const requestItemStates = loaded.collections.RequestItemStates as RequestItemState[];
        this.draftRequestItem = requestItemStates.find((v: RequestItemState) => v.UniqueId === 'b173dfbe-9421-4697-8ffb-e46afc724490');
        this.submittedRequestItem = requestItemStates.find((v: RequestItemState) => v.UniqueId === 'b118c185-de34-4131-be1f-e6162c1dea4b');

        const requestStates = loaded.collections.RequestStates as RequestState[];
        this.anonymousRequest = requestStates.find((v: RequestState) => v.UniqueId === '2f054949-e30c-4954-9a3c-191559de8315');
        this.submittedRequest = requestStates.find((v: RequestState) => v.UniqueId === 'db03407d-bcb1-433a-b4e9-26cea9a71bfd');
        this.pendingCustomerRequest = requestStates.find((v: RequestState) => v.UniqueId === '671fda2f-5aa6-4ea5-b5d6-c914f0911690');

        const quoteItemStates = loaded.collections.QuoteItemStates as QuoteItemState[];
        this.draftQuoteItem = quoteItemStates.find((v: QuoteItemState) => v.UniqueId === '84ad17a3-10f7-4fdb-b76a-41bdb1edb0e6');
        this.submittedQuoteItem = quoteItemStates.find((v: QuoteItemState) => v.UniqueId === 'e511ea2d-6eb9-428d-a982-b097938a8ff8');
        this.approvedQuoteItem = quoteItemStates.find((v: QuoteItemState) => v.UniqueId === '3335810c-9e26-4604-b272-d18b831e79e0');

        const quoteStates = loaded.collections.QuoteStates as QuoteState[];
        this.createdQuote = quoteStates.find((v: QuoteState) => v.UniqueId === 'b1565cd4-d01a-4623-bf19-8c816df96aa6');
        this.approvedQuote = quoteStates.find((v: QuoteState) => v.UniqueId === '675d6899-1ebb-4fdb-9dc9-b8aef0a135d2');

        const shipmentItemStates = loaded.collections.ShipmentItemStates as ShipmentItemState[];
        this.createdShipmentItem = shipmentItemStates.find((v: ShipmentItemState) => v.UniqueId === 'e05818b1-2660-4879-b5a8-8ca96f324f7b');
        this.pickingShipmentItem = shipmentItemStates.find((v: ShipmentItemState) => v.UniqueId === 'f9043add-e106-4646-8b02-6b10efbb2e87');
        this.pickedShipmentItem = shipmentItemStates.find((v: ShipmentItemState) => v.UniqueId === 'a8e2014f-c4cb-4a6f-8ccf-0875e439d1f3');
        this.packedShipmentItem = shipmentItemStates.find((v: ShipmentItemState) => v.UniqueId === '91853258-c875-4f85-bd84-ef1ebd2e5930');

        const shipmentStates = loaded.collections.ShipmentStates as ShipmentState[];
        this.createdShipment = shipmentStates.find((v: ShipmentState) => v.UniqueId === '854ad6a0-b2d1-4b92-8c3d-e9e72dd19afd');
        this.pickingShipment = shipmentStates.find((v: ShipmentState) => v.UniqueId === '1d76de65-4de4-494d-8677-653b4d62aa42');
        this.pickedShipment = shipmentStates.find((v: ShipmentState) => v.UniqueId === 'c63c5d25-f139-490f-86d1-2e9e51f5c0a5');
        this.packedShipment = shipmentStates.find((v: ShipmentState) => v.UniqueId === 'dcabe845-a6f2-49d9-bbae-06fb47012a21');
        this.onholdShipment = shipmentStates.find((v: ShipmentState) => v.UniqueId === '268cb9a7-6965-47e8-af89-8f915242c23d');

        const purchaseOrderStates = loaded.collections.PurchaseOrderStates as PurchaseOrderState[];
        const purchaseOrderinProcess = purchaseOrderStates.find((v) => v.UniqueId === '7752f5c5-b19b-4339-a937-0bad768142a8');

        const salesOrderItems = loaded.collections.SalesOrderItems as SalesOrderItem[];
        if (salesOrderItems) {
          this.salesOrderItems = salesOrderItems.filter(v => v.SalesOrderWhereSalesOrderItem.SalesOrderState === inProcess && parseFloat(v.QuantityRequestsShipping) > 0);
        }

        const purchaseOrderItems = loaded.collections.PurchaseOrderItems as PurchaseOrderItem[];
        if (purchaseOrderItems) {
          this.purchaseOrderItems = purchaseOrderItems.filter(v => v.PurchaseOrderWherePurchaseOrderItem.PurchaseOrderState === purchaseOrderinProcess);
        }

        if (this.isPurchaseShipment) {
          this.supplierOfferings = loaded.collections.SupplierOfferings as SupplierOffering[];

          this.parts = this.supplierOfferings
            .filter(v => v.Supplier === this.shipment.ShipFromParty && v.Supplier === this.shipment.ShipFromParty && moment(v.FromDate).isBefore(now) && (v.ThroughDate === null || moment(v.ThroughDate).isAfter(now)))
            .map(v => v.Part)
            .sort((a, b) => (a.Name > b.Name) ? 1 : ((b.Name > a.Name) ? -1 : 0));
        }

        if (isCreate) {
          this.title = 'Add Shipment Item';
          this.shipmentItem = this.allors.context.create('ShipmentItem') as ShipmentItem;
          this.shipment.AddShipmentItem(this.shipmentItem);

          if (this.isCustomerShipment) {
            this.orderShipment = this.allors.context.create('OrderShipment') as OrderShipment;
          }

          if (this.isPurchaseShipment) {
            this.shipmentReceipt = this.allors.context.create('ShipmentReceipt') as ShipmentReceipt;
          }
        } else {

          // This UI does not allow shipmentitem to be combination from multiple order items
          const orderShipments = loaded.collections.OrderShipments as OrderShipment[];
          if (orderShipments && orderShipments.length > 0) {
            this.orderShipment = orderShipments[0];
            if (this.orderShipment.OrderItem) {
              if (this.isCustomerShipment) {
                this.selectedSalesOrderItem = this.orderShipment.OrderItem as SalesOrderItem;
                this.salesOrderItems.push(this.selectedSalesOrderItem);
              }

              if (this.isPurchaseShipment) {
                this.selectedPurchaseOrderItem = this.orderShipment.OrderItem as PurchaseOrderItem;
                this.purchaseOrderItems.push(this.selectedPurchaseOrderItem);
              }
            }
          }

          this.shipmentReceipt = loaded.objects.ShipmentReceipt as ShipmentReceipt;

          if (this.shipmentItem.Good) {
            this.previousGood = this.shipmentItem.Good;
            this.loadProduct(this.shipmentItem.Good);
          }

          if (this.shipmentItem.Part) {
            this.previousPart = this.shipmentItem.Part;
            this.loadPart(this.shipmentItem.Part);
          }

          this.serialisedItems.push(this.shipmentItem.SerialisedItem);

          if (this.shipmentItem.CanWriteQuantity) {
            this.title = 'Edit Shipment Order Item';
          } else {
            this.title = 'View Shipment Order Item';
          }
        }
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {

    this.onSave();

    this.allors.context.save()
      .subscribe(() => {
        const data: IObject = {
          id: this.shipmentItem.id,
          objectType: this.shipmentItem.objectType,
        };

        this.dialogRef.close(data);
      },
        this.saveService.errorHandler
      );
  }

  public salesOrderItemSelected(salesOrderItem: SalesOrderItem): void {

    this.shipmentItem.Good = salesOrderItem.Product as Good;
    this.shipmentItem.Quantity = salesOrderItem.QuantityRequestsShipping;
    this.shipmentItem.SerialisedItem = salesOrderItem.SerialisedItem;
  }

  public purchaseOrderItemSelected(purchaseOrderItem: PurchaseOrderItem): void {

    this.shipmentItem.Part = purchaseOrderItem.Part as Part;
    this.shipmentItem.Quantity = purchaseOrderItem.QuantityOrdered;
    this.shipmentItem.SerialisedItem = purchaseOrderItem.SerialisedItem;
  }

  public goodSelected(product: Product): void {
    if (product) {
      this.loadProduct(product);
    }
  }

  public partSelected(part: Part): void {
    if (part) {
      this.loadPart(part);
    }
  }

  public serialisedItemSelected(serialisedItem: SerialisedItem): void {

    const onRequestItem = serialisedItem.RequestItemsWhereSerialisedItem
      .find(v => (v.RequestItemState === this.draftRequestItem || v.RequestItemState === this.submittedRequestItem)
        && (v.RequestWhereRequestItem.RequestState === this.anonymousRequest || v.RequestWhereRequestItem.RequestState === this.submittedRequest || v.RequestWhereRequestItem.RequestState === this.pendingCustomerRequest));

    const onQuoteItem = serialisedItem.QuoteItemsWhereSerialisedItem
      .find(v => (v.QuoteItemState === this.draftQuoteItem || v.QuoteItemState === this.submittedQuoteItem || v.QuoteItemState === this.approvedQuoteItem)
        && (v.QuoteWhereQuoteItem.QuoteState === this.createdQuote || v.QuoteWhereQuoteItem.QuoteState === this.approvedQuote));

    const onOrderItem = serialisedItem.SalesOrderItemsWhereSerialisedItem
      .find(v => (v.SalesOrderItemState === this.createdOrderItem || v.SalesOrderItemState === this.onHoldOrderItem || v.SalesOrderItemState === this.inProcessOrderItem)
        && (v.SalesOrderWhereSalesOrderItem.SalesOrderState === this.provisionalOrder || v.SalesOrderWhereSalesOrderItem.SalesOrderState === this.requestsApprovalOrder)
        || v.SalesOrderWhereSalesOrderItem.SalesOrderState === this.onHoldOrder || v.SalesOrderWhereSalesOrderItem.SalesOrderState === this.inProcessOrder);

    const onOtherShipmentItem = serialisedItem.ShipmentItemsWhereSerialisedItem
      .find(v => (v.ShipmentItemState === this.createdShipmentItem || v.ShipmentItemState === this.pickingShipmentItem || v.ShipmentItemState === this.pickedShipmentItem || v.ShipmentItemState === this.packedShipmentItem)
        && (v.ShipmentWhereShipmentItem.ShipmentState === this.createdShipment || v.ShipmentWhereShipmentItem.ShipmentState === this.pickingShipment)
        || v.ShipmentWhereShipmentItem.ShipmentState === this.pickingShipment || v.ShipmentWhereShipmentItem.ShipmentState === this.packedShipment
        || v.ShipmentWhereShipmentItem.ShipmentState === this.onholdShipment);

    if (onRequestItem) {
      this.snackBar.open(`Item already requested with ${onRequestItem.RequestWhereRequestItem.RequestNumber}`, 'close');
    }

    if (onQuoteItem) {
      this.snackBar.open(`Item already quoted with ${onQuoteItem.QuoteWhereQuoteItem.QuoteNumber}`, 'close');
    }

    if (onOrderItem) {
      this.snackBar.open(`Item already ordered with ${onOrderItem.SalesOrderWhereSalesOrderItem.OrderNumber}`, 'close');
    }

    if (onOtherShipmentItem) {
      this.snackBar.open(`Item already shipped with ${onOtherShipmentItem.ShipmentWhereShipmentItem.ShipmentNumber}`, 'close');
    }

    this.shipmentItem.Quantity = '1';
  }

  private loadProduct(product: Product): void {

    const { pull, x } = this.metaService;

    const pulls = [
      pull.NonUnifiedGood({
        object: product.id,
        fetch: {
          Part: {
            include: {
              SerialisedItems: {
                RequestItemsWhereSerialisedItem: {
                  RequestItemState: x,
                  RequestWhereRequestItem: {
                    RequestState: x
                  }
                },
                QuoteItemsWhereSerialisedItem: {
                  QuoteItemState: x,
                  QuoteWhereQuoteItem: {
                    QuoteState: x
                  }
                },
                SalesOrderItemsWhereSerialisedItem: {
                  SalesOrderItemState: x,
                  SalesOrderWhereSalesOrderItem: {
                    SalesOrderState: x
                  }
                },
                ShipmentItemsWhereSerialisedItem: {
                  ShipmentItemState: x,
                  ShipmentWhereShipmentItem: {
                    ShipmentState: x
                  }
                }
              }
            }
          }
        }
      }),
      pull.UnifiedGood({
        object: product.id,
        include: {
          SerialisedItems: {
            RequestItemsWhereSerialisedItem: {
              RequestItemState: x,
              RequestWhereRequestItem: {
                RequestState: x
              }
            },
            QuoteItemsWhereSerialisedItem: {
              QuoteItemState: x,
              QuoteWhereQuoteItem: {
                QuoteState: x
              }
            },
            SalesOrderItemsWhereSerialisedItem: {
              SalesOrderItemState: x,
              SalesOrderWhereSalesOrderItem: {
                SalesOrderState: x
              }
            },
            ShipmentItemsWhereSerialisedItem: {
              ShipmentItemState: x,
              ShipmentWhereShipmentItem: {
                ShipmentState: x
              }
            }
          }
        }
      })
    ];

    this.allors.context
      .load(new PullRequest({ pulls }))
      .subscribe((loaded) => {

        const part = (loaded.objects.UnifiedGood || loaded.objects.Part) as Part;

        if (this.isCustomerShipment) {
          this.serialisedItems = part.SerialisedItems.filter(v => v.AvailableForSale === true);
        }

        if (this.shipmentItem.Good !== this.previousGood) {
          this.shipmentItem.SerialisedItem = null;
          this.previousGood = this.shipmentItem.Good;
        }
      });
  }

  private loadPart(part: Part): void {

    const { pull, x } = this.metaService;

    const pulls = [
      pull.NonUnifiedPart({
        object: part.id,
        fetch: {
          InventoryItemsWherePart: {
            include: {
              Facility: x
            }
          }
        }
      }),
    ];

    this.allors.context
      .load(new PullRequest({ pulls }))
      .subscribe((loaded) => {

        this.inventoryItems = loaded.collections.InventoryItems as InventoryItem[];

        if (this.shipmentItem.Part !== this.previousPart) {
          this.shipmentItem.SerialisedItem = null;
          this.previousPart = this.shipmentItem.Part;
        }
      });
  }

  private onSave() {
    if (this.selectedSalesOrderItem) {
      this.orderShipment.OrderItem = this.selectedSalesOrderItem;
      this.orderShipment.ShipmentItem = this.shipmentItem;
      this.orderShipment.Quantity = this.shipmentItem.Quantity;
    }

    if (this.isPurchaseShipment) {
      this.shipmentReceipt.OrderItem = this.selectedPurchaseOrderItem;
      this.shipmentReceipt.ShipmentItem = this.shipmentItem;
      this.shipmentReceipt.QuantityAccepted = this.shipmentItem.Quantity;
    }
  }
}
