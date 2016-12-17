// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PurchaseOrder.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
// 
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// 
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// 
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using Meta;
    using Resources;

    public partial class PurchaseOrder
    {
        ObjectState Transitional.CurrentObjectState => this.CurrentObjectState;

        public bool IsProvisional => this.CurrentObjectState.Equals(new PurchaseOrderObjectStates(this.Strategy.Session).Provisional);

        public OrderItem[] OrderItems => this.PurchaseOrderItems;

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistCurrentObjectState)
            {
                this.CurrentObjectState = new PurchaseOrderObjectStates(this.Strategy.Session).Provisional;
            }

            if (this.ExistTakenViaSupplier)
            {
                this.PreviousTakenViaSupplier = this.TakenViaSupplier;
            }

            if (!this.ExistBillToPurchaser)
            {
                this.BillToPurchaser = Singleton.Instance(this.Strategy.Session).DefaultInternalOrganisation;
            }

            if (!this.ExistShipToBuyer)
            {
                this.ShipToBuyer = Singleton.Instance(this.Strategy.Session).DefaultInternalOrganisation;
            }

            if (!this.ExistOrderNumber && this.ExistBillToPurchaser)
            {
                this.OrderNumber = this.BillToPurchaser.DeriveNextPurchaseOrderNumber();
            }

            if (!this.ExistOrderDate)
            {
                this.OrderDate = DateTime.UtcNow;
            }

            if (!this.ExistEntryDate)
            {
                this.EntryDate = DateTime.UtcNow;
            }

            if (!this.ExistCustomerCurrency)
            {
                if (this.ExistShipToBuyer)
                {
                    this.CustomerCurrency = this.ShipToBuyer.ExistPreferredCurrency ? this.ShipToBuyer.PreferredCurrency : this.ShipToBuyer.Locale.Country.Currency;
                }
            }

            if (!this.ExistFacility)
            {
                if (Singleton.Instance(this.Strategy.Session).DefaultInternalOrganisation != null)
                {
                    this.Facility = Singleton.Instance(this.Strategy.Session).DefaultInternalOrganisation.DefaultFacility;
                }
            }
        }

        public void AppsOnPreDerive(ObjectOnPreDerive method)
        {
            var derivation = method.Derivation;

            // TODO:
            if (derivation.ChangeSet.Associations.Contains(this.Id))
            {
                if (this.ExistBillToPurchaser)
                {
                    derivation.AddDependency(this, this.BillToPurchaser);
                }

                if (this.ExistShipToBuyer)
                {
                    derivation.AddDependency(this, this.ShipToBuyer);
                }

                if (this.ExistTakenViaSupplier)
                {
                    derivation.AddDependency(this, this.TakenViaSupplier);
                }
            }

            if (this.ExistTakenViaSupplier)
            {
                var supplier = this.TakenViaSupplier as Organisation;
                if (supplier != null)
                {
                    var supplierRelationships = supplier.SupplierRelationshipsWhereSupplier;
                    supplierRelationships.Filter.AddEquals(M.SupplierRelationship.InternalOrganisation, this.ShipToBuyer);

                    foreach (SupplierRelationship supplierRelationship in supplierRelationships)
                    {
                        if (supplierRelationship.FromDate <= DateTime.UtcNow && (!supplierRelationship.ExistThroughDate || supplierRelationship.ThroughDate >= DateTime.UtcNow))
                        {
                            derivation.AddDependency(this, supplierRelationship);
                        }
                    }
                }
            }

            foreach (PurchaseOrderItem orderItem in this.OrderItems)
            {
                derivation.AddDependency(this, orderItem);
            }
        }

        public void AppsOnDerive(ObjectOnDerive method)
        {
            var derivation = method.Derivation;

            Organisation supplier = this.TakenViaSupplier as Organisation;
            if (supplier != null && this.ExistShipToBuyer)
            {
                if (!this.ShipToBuyer.Equals(supplier.InternalOrganisationWhereSupplier))
                {
                    derivation.Validation.AddError(this, M.PurchaseOrder.TakenViaSupplier, ErrorMessages.PartyIsNotASupplier);
                }
            }

            if (!this.ExistShipToAddress && this.ExistShipToBuyer)
            {
                this.ShipToAddress = this.ShipToBuyer.ShippingAddress;
            }

            if (!this.ExistBillToContactMechanism && this.ExistBillToPurchaser)
            {
                this.BillToContactMechanism = this.BillToPurchaser.BillingAddress;
            }

            if (!this.ExistTakenViaContactMechanism && this.ExistTakenViaSupplier)
            {
                this.TakenViaContactMechanism = this.TakenViaSupplier.OrderAddress;
            }

            this.AppsOnDeriveOrderItems(derivation);

            this.AppsOnDeriveCurrentShipmentStatus(derivation);
            this.AppsOnDeriveCurrentPaymentStatus(derivation);
            this.DeriveCurrentObjectState(derivation);
            this.AppsOnDeriveLocale(derivation);
            this.AppsOnDeriveOrderTotals(derivation);

            this.PreviousTakenViaSupplier = this.TakenViaSupplier;
        }

        public void AppsOnPostDerive(ObjectOnPostDerive method)
        {
            this.RemoveSecurityTokens();
            this.AddSecurityToken(Singleton.Instance(this.Strategy.Session).DefaultSecurityToken);

            if (this.ExistBillToPurchaser)
            {
                var securityToken = this.BillToPurchaser.OwnerSecurityToken;

                if (!this.SecurityTokens.Contains(securityToken))
                {
                    this.AddSecurityToken(securityToken);
                }
            }

            if (this.ExistShipToBuyer)
            {
                var securityToken = this.ShipToBuyer.OwnerSecurityToken;

                if (!this.SecurityTokens.Contains(securityToken))
                {
                    this.AddSecurityToken(securityToken);
                }
            }

            if (this.ExistTakenViaSupplier)
            {
                var securityToken = this.TakenViaSupplier.OwnerSecurityToken;

                if (!this.SecurityTokens.Contains(securityToken))
                {
                    this.AddSecurityToken(securityToken);
                }
            }

            if (this.ExistPreviousTakenViaSupplier && !this.TakenViaSupplier.Equals(this.PreviousTakenViaSupplier))
            {
                this.RemoveSecurityToken(this.PreviousTakenViaSupplier.OwnerSecurityToken);
            }
        }

        private void DeriveCurrentObjectState(IDerivation derivation)
        {
            if (this.ExistCurrentObjectState && !this.CurrentObjectState.Equals(this.LastObjectState))
            {
                var currentStatus = new PurchaseOrderStatusBuilder(this.Strategy.Session).WithPurchaseOrderObjectState(this.CurrentObjectState).Build();
                this.AddOrderStatus(currentStatus);
                this.CurrentOrderStatus = currentStatus;
            }
        }

        public void AppsCancel(OrderCancel method)
        {
            this.CurrentObjectState = new PurchaseOrderObjectStates(this.Strategy.Session).Cancelled;
        }

        public void AppsConfirm(OrderConfirm method)
        {
            this.CurrentObjectState = new PurchaseOrderObjectStates(this.Strategy.Session).InProcess;
        }

        public void AppsReject(OrderReject method)
        {
            this.CurrentObjectState = new PurchaseOrderObjectStates(this.Strategy.Session).Rejected;
        }

        public void AppsHold(OrderHold method)
        {
            this.CurrentObjectState = new PurchaseOrderObjectStates(this.Strategy.Session).OnHold;
        }

        public void AppsApprove(OrderApprove method)
        {
            this.CurrentObjectState = new PurchaseOrderObjectStates(this.Strategy.Session).RequestsApproval;
        }

        public void AppsContinue(OrderContinue method)
        {
            this.CurrentObjectState = new PurchaseOrderObjectStates(this.Strategy.Session).InProcess;
        }

        public void AppsComplete(OrderComplete method)
        {
            this.CurrentObjectState = new PurchaseOrderObjectStates(this.Strategy.Session).Completed;
        }

        public void AppsFinish(OrderFinish method)
        {
            this.CurrentObjectState = new PurchaseOrderObjectStates(this.Strategy.Session).Finished;
        }

        public void AppsOnDeriveCurrentPaymentStatus(IDerivation derivation)
        {
            var itemsPaid = false;
            var itemsPartiallyPaid = false;
            var itemsUnpaid = false;
            foreach (PurchaseOrderItem orderItem in this.ValidOrderItems)
            {
                if (orderItem.ExistCurrentPaymentStatus)
                {
                    if (orderItem.CurrentPaymentStatus.PurchaseOrderItemObjectState.Equals(new PurchaseOrderItemObjectStates(this.Strategy.Session).PartiallyPaid))
                    {
                        itemsPartiallyPaid = true;
                    }

                    if (orderItem.CurrentPaymentStatus.PurchaseOrderItemObjectState.Equals(new PurchaseOrderItemObjectStates(this.Strategy.Session).Paid))
                    {
                        itemsPaid = true;
                    }
                }
                else
                {
                    itemsUnpaid = true;
                }
            }

            if (itemsPaid && !itemsUnpaid && !itemsPartiallyPaid &&
                (!this.ExistCurrentPaymentStatus || !this.CurrentPaymentStatus.PurchaseOrderObjectState.Equals(new PurchaseOrderObjectStates(this.Strategy.Session).Paid)))
            {
                this.CurrentPaymentStatus = new PurchaseOrderStatusBuilder(this.Strategy.Session).WithPurchaseOrderObjectState(new PurchaseOrderObjectStates(this.Strategy.Session).Paid).Build();
            }

            if ((itemsPartiallyPaid || (itemsPaid && itemsUnpaid)) &&
                (!this.ExistCurrentPaymentStatus || !this.CurrentPaymentStatus.PurchaseOrderObjectState.Equals(new PurchaseOrderObjectStates(this.Strategy.Session).PartiallyPaid)))
            {
                this.CurrentPaymentStatus = new PurchaseOrderStatusBuilder(this.Strategy.Session).WithPurchaseOrderObjectState(new PurchaseOrderObjectStates(this.Strategy.Session).PartiallyPaid).Build();
            }

            this.AppsOnDeriveCurrentOrderStatus(derivation);
        }

        public void AppsOnDeriveCurrentShipmentStatus(IDerivation derivation)
        {
            var itemsShipped = false;
            var itemsPartiallyShipped = false;
            var itemsNotShipped = false;
            foreach (PurchaseOrderItem orderItem in this.ValidOrderItems)
            {
                if (orderItem.ExistCurrentShipmentStatus)
                {
                    if (orderItem.CurrentShipmentStatus.PurchaseOrderItemObjectState.Equals(new PurchaseOrderItemObjectStates(this.Strategy.Session).PartiallyReceived))
                    {
                        itemsPartiallyShipped = true;
                    }

                    if (orderItem.CurrentShipmentStatus.PurchaseOrderItemObjectState.Equals(new PurchaseOrderItemObjectStates(this.Strategy.Session).Received))
                    {
                        itemsShipped = true;
                    }
                }
                else
                {
                    itemsNotShipped = true;
                }
            }

            if (itemsShipped && !itemsNotShipped && !itemsPartiallyShipped &&
                (!this.ExistCurrentShipmentStatus || !this.CurrentShipmentStatus.PurchaseOrderObjectState.Equals(new PurchaseOrderObjectStates(this.Strategy.Session).Received)))
            {
                this.CurrentShipmentStatus = new PurchaseOrderStatusBuilder(this.Strategy.Session).WithPurchaseOrderObjectState(new PurchaseOrderObjectStates(this.Strategy.Session).Received).Build();
            }

            if ((itemsPartiallyShipped || (itemsShipped && itemsNotShipped)) &&
                (!this.ExistCurrentShipmentStatus || !this.CurrentShipmentStatus.PurchaseOrderObjectState.Equals(new PurchaseOrderObjectStates(this.Strategy.Session).PartiallyReceived)))
            {
                this.CurrentShipmentStatus = new PurchaseOrderStatusBuilder(this.Strategy.Session).WithPurchaseOrderObjectState(new PurchaseOrderObjectStates(this.Strategy.Session).PartiallyReceived).Build();
            }

            this.AppsOnDeriveCurrentOrderStatus(derivation);
        }

        public void AppsOnDeriveCurrentOrderStatus(IDerivation derivation)
        {
            if (this.ExistCurrentShipmentStatus && this.CurrentShipmentStatus.PurchaseOrderObjectState.Equals(new PurchaseOrderObjectStates(this.Strategy.Session).Received))
            {
                this.Complete();
            }

            if (this.ExistCurrentPaymentStatus && this.CurrentPaymentStatus.PurchaseOrderObjectState.Equals(new PurchaseOrderObjectStates(this.Strategy.Session).Paid))
            {
                this.Finish();
            }
        }

        public void AppsOnDeriveLocale(IDerivation derivation)
        {
            this.Locale = this.ExistShipToBuyer && this.ShipToBuyer.ExistLocale
                              ? this.ShipToBuyer.Locale
                              : Singleton.Instance(this.Strategy.Session).DefaultLocale;
        }

        public void AppsOnDeriveOrderTotals(IDerivation derivation)
        {
            if (this.ExistValidOrderItems)
            {
                this.TotalBasePrice = 0;
                this.TotalDiscount = 0;
                this.TotalSurcharge = 0;
                this.TotalVat = 0;
                this.TotalExVat = 0;
                this.TotalIncVat = 0;

                foreach (PurchaseOrderItem orderItem in this.ValidOrderItems)
                {
                    this.TotalBasePrice += orderItem.TotalBasePrice;
                    this.TotalDiscount += orderItem.TotalDiscount;
                    this.TotalSurcharge += orderItem.TotalSurcharge;
                    this.TotalVat += orderItem.TotalVat;
                    this.TotalExVat += orderItem.TotalExVat;
                    this.TotalIncVat += orderItem.TotalIncVat;
                }
            }
        }

        public void AppsOnDeriveOrderItems(IDerivation derivation)
        {
            var quantityOrderedByProduct = new Dictionary<Product, decimal>();
            var totalBasePriceByProduct = new Dictionary<Product, decimal>();
            var quantityOrderedByPart = new Dictionary<Part, decimal>();
            var totalBasePriceByPart = new Dictionary<Part, decimal>();

            foreach (PurchaseOrderItem purchaseOrderItem in this.ValidOrderItems)
            {
                purchaseOrderItem.OnDerive(x => x.WithDerivation(derivation));
                purchaseOrderItem.AppsOnDeriveDeliveryDate(derivation);
                purchaseOrderItem.AppsOnDeriveCurrentShipmentStatus(derivation);
                purchaseOrderItem.AppsOnDerivePrices();
                purchaseOrderItem.AppsDeriveVatRegime(derivation);


                if (purchaseOrderItem.ExistProduct)
                {
                    if (!quantityOrderedByProduct.ContainsKey(purchaseOrderItem.Product))
                    {
                        quantityOrderedByProduct.Add(purchaseOrderItem.Product, purchaseOrderItem.QuantityOrdered);
                        totalBasePriceByProduct.Add(purchaseOrderItem.Product, purchaseOrderItem.TotalBasePrice);
                    }
                    else
                    {
                        quantityOrderedByProduct[purchaseOrderItem.Product] += purchaseOrderItem.QuantityOrdered;
                        totalBasePriceByProduct[purchaseOrderItem.Product] += purchaseOrderItem.TotalBasePrice;
                    }
                }

                if (purchaseOrderItem.ExistPart)
                {
                    if (!quantityOrderedByPart.ContainsKey(purchaseOrderItem.Part))
                    {
                        quantityOrderedByPart.Add(purchaseOrderItem.Part, purchaseOrderItem.QuantityOrdered);
                        totalBasePriceByPart.Add(purchaseOrderItem.Part, purchaseOrderItem.TotalBasePrice);
                    }
                    else
                    {
                        quantityOrderedByPart[purchaseOrderItem.Part] += purchaseOrderItem.QuantityOrdered;
                        totalBasePriceByPart[purchaseOrderItem.Part] += purchaseOrderItem.TotalBasePrice;
                    }
                }
            }
        }
    }
}