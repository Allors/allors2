// <copyright file="SalesOrder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;
    using Allors.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Resources;

    public partial class SalesOrder
    {
        public static readonly TransitionalConfiguration[] StaticTransitionalConfigurations =
            {
                new TransitionalConfiguration(M.SalesOrder, M.SalesOrder.SalesOrderState),
                new TransitionalConfiguration(M.SalesOrder, M.SalesOrder.SalesOrderShipmentState),
                new TransitionalConfiguration(M.SalesOrder, M.SalesOrder.SalesOrderInvoiceState),
                new TransitionalConfiguration(M.SalesOrder, M.SalesOrder.SalesOrderPaymentState),
            };

        public TransitionalConfiguration[] TransitionalConfigurations => StaticTransitionalConfigurations;

        public int PaymentNetDays
        {
            get
            {
                if (this.ExistSalesTerms)
                {
                    foreach (AgreementTerm term in this.SalesTerms)
                    {
                        if (term.TermType.Equals(new InvoiceTermTypes(this.Strategy.Session).PaymentNetDays))
                        {
                            if (int.TryParse(term.TermValue, out var netDays))
                            {
                                return netDays;
                            }
                        }
                    }
                }

                if (this.ExistBillToCustomer && this.BillToCustomer.PaymentNetDays().HasValue)
                {
                    return this.BillToCustomer.PaymentNetDays().Value;
                }

                if (this.ExistStore && this.Store.ExistPaymentNetDays)
                {
                    return this.Store.PaymentNetDays;
                }

                return 0;
            }
        }

        public void BaseOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistSalesOrderState)
            {
                this.SalesOrderState = new SalesOrderStates(this.Strategy.Session).Provisional;
            }

            if (!this.ExistSalesOrderShipmentState)
            {
                this.SalesOrderShipmentState = new SalesOrderShipmentStates(this.Strategy.Session).NotShipped;
            }

            if (!this.ExistSalesOrderInvoiceState)
            {
                this.SalesOrderInvoiceState = new SalesOrderInvoiceStates(this.Strategy.Session).NotInvoiced;
            }

            if (!this.ExistSalesOrderPaymentState)
            {
                this.SalesOrderPaymentState = new SalesOrderPaymentStates(this.Strategy.Session).NotPaid;
            }

            if (!this.ExistOrderDate)
            {
                this.OrderDate = this.Session().Now();
            }

            if (!this.ExistEntryDate)
            {
                this.EntryDate = this.Session().Now();
            }

            if (!this.ExistPartiallyShip)
            {
                this.PartiallyShip = true;
            }

            if (!this.ExistTakenBy)
            {
                var internalOrganisations = new Organisations(this.Strategy.Session).InternalOrganisations();
                if (internalOrganisations.Count() == 1)
                {
                    this.TakenBy = internalOrganisations.First();
                }
            }

            if (!this.ExistStore && this.ExistTakenBy)
            {
                var stores = new Stores(this.Strategy.Session).Extent();
                stores.Filter.AddEquals(M.Store.InternalOrganisation, this.TakenBy);

                if (stores.Any())
                {
                    this.Store = stores.First;
                }
            }

            if (!this.ExistOriginFacility)
            {
                this.OriginFacility = this.ExistStore ? this.Store.DefaultFacility : this.Strategy.Session.GetSingleton().Settings.DefaultFacility;
            }

            if (!this.ExistOrderNumber && this.ExistStore)
            {
                this.OrderNumber = this.Store.NextSalesOrderNumber(this.OrderDate.Year);
            }
        }

        public void BaseOnPreDerive(ObjectOnPreDerive method)
        {
            var (iteration, changeSet, derivedObjects) = method;

            if (iteration.IsMarked(this) || changeSet.IsCreated(this) || changeSet.HasChangedRoles(this))
            {
                iteration.AddDependency(this.BillToCustomer, this);
                iteration.Mark(this.BillToCustomer);

                iteration.AddDependency(this.ShipToCustomer, this);
                iteration.Mark(this.ShipToCustomer);

                foreach (SalesOrderItem orderItem in this.SalesOrderItems)
                {
                    iteration.AddDependency(this, orderItem);
                    iteration.Mark(orderItem);
                }
            }
        }

        public void BaseOnDerive(ObjectOnDerive method)
        {
            var derivation = method.Derivation;
            var session = this.Session();

            // SalesOrder Derivations and Validations
            this.BillToCustomer ??= this.ShipToCustomer;
            this.ShipToCustomer ??= this.BillToCustomer;
            this.Customers = new[] { this.BillToCustomer, this.ShipToCustomer, this.PlacingCustomer };
            this.Locale ??= this.BillToCustomer?.Locale ?? this.Strategy.Session.GetSingleton().DefaultLocale;
            this.VatRegime ??= this.BillToCustomer?.VatRegime;
            this.Currency ??= this.BillToCustomer?.PreferredCurrency ?? this.BillToCustomer?.Locale?.Country?.Currency ?? this.TakenBy?.PreferredCurrency;
            this.TakenByContactMechanism ??= this.TakenBy?.OrderAddress ?? this.TakenBy?.BillingAddress ?? this.TakenBy?.GeneralCorrespondence;
            this.BillToContactMechanism ??= this.BillToCustomer?.BillingAddress ?? this.BillToCustomer?.ShippingAddress ?? this.BillToCustomer?.GeneralCorrespondence;
            this.BillToEndCustomerContactMechanism ??= this.BillToEndCustomer?.BillingAddress ?? this.BillToEndCustomer?.ShippingAddress ?? this.BillToCustomer?.GeneralCorrespondence;
            this.ShipToEndCustomerAddress ??= this.ShipToEndCustomer?.ShippingAddress ?? this.ShipToCustomer?.GeneralCorrespondence as PostalAddress;
            this.ShipFromAddress ??= this.TakenBy?.ShippingAddress;
            this.ShipToAddress ??= this.ShipToCustomer?.ShippingAddress;
            this.ShipmentMethod ??= this.ShipToCustomer?.DefaultShipmentMethod ?? this.Store.DefaultShipmentMethod;
            this.PaymentMethod ??= this.ShipToCustomer?.PartyFinancialRelationshipsWhereParty?.FirstOrDefault(v => object.Equals(v.InternalOrganisation, this.TakenBy))?.DefaultPaymentMethod ?? this.Store.DefaultCollectionMethod;

            if (this.BillToCustomer?.BaseIsActiveCustomer(this.TakenBy, this.OrderDate) == false)
            {
                derivation.Validation.AddError(this, M.SalesOrder.BillToCustomer, ErrorMessages.PartyIsNotACustomer);
            }

            if (this.ShipToCustomer?.BaseIsActiveCustomer(this.TakenBy, this.OrderDate) == false)
            {
                derivation.Validation.AddError(this, M.SalesOrder.ShipToCustomer, ErrorMessages.PartyIsNotACustomer);
            }

            if (this.SalesOrderState.InProcess)
            {
                derivation.Validation.AssertExists(this, this.Meta.ShipToAddress);
                derivation.Validation.AssertExists(this, this.Meta.BillToContactMechanism);
            }

            // SalesOrderItem Derivations and Validations
            foreach (SalesOrderItem salesOrderItem in this.SalesOrderItems)
            {
                var salesOrderItemDerivedRoles = (SalesOrderItemDerivedRoles)salesOrderItem;

                salesOrderItem.ShipFromAddress ??= this.ShipFromAddress;
                salesOrderItemDerivedRoles.ShipToAddress = salesOrderItem.AssignedShipToAddress ?? salesOrderItem.AssignedShipToParty?.ShippingAddress ?? this.ShipToAddress;
                salesOrderItemDerivedRoles.ShipToParty = salesOrderItem.AssignedShipToParty ?? this.ShipToCustomer;
                salesOrderItemDerivedRoles.DeliveryDate = salesOrderItem.AssignedDeliveryDate ?? this.DeliveryDate;
                salesOrderItemDerivedRoles.VatRegime = salesOrderItem.AssignedVatRegime ?? this.VatRegime;
                salesOrderItemDerivedRoles.VatRate = salesOrderItem.VatRegime?.VatRate ?? salesOrderItem.Product?.VatRate ?? salesOrderItem.ProductFeature?.VatRate;

                salesOrderItemDerivedRoles.SalesReps = salesOrderItem.Product?.ProductCategoriesWhereProduct.Select(v => SalesRepRelationships.SalesRep(salesOrderItem.ShipToParty, v, this.OrderDate)).ToArray();
                salesOrderItemDerivedRoles.AddSalesRep(SalesRepRelationships.SalesRep(salesOrderItem.ShipToParty, null, this.OrderDate));

                // TODO: Use versioning
                if (salesOrderItem.ExistPreviousProduct && !salesOrderItem.PreviousProduct.Equals(salesOrderItem.Product))
                {
                    derivation.Validation.AddError(salesOrderItem, M.SalesOrderItem.Product, ErrorMessages.SalesOrderItemProductChangeNotAllowed);
                }
                else
                {
                    salesOrderItemDerivedRoles.PreviousProduct = salesOrderItem.Product;
                }

                if (salesOrderItem.ExistSalesOrderItemWhereOrderedWithFeature)
                {
                    derivation.Validation.AssertExists(salesOrderItem, M.SalesOrderItem.ProductFeature);
                    derivation.Validation.AssertNotExists(salesOrderItem, M.SalesOrderItem.Product);
                }
                else
                {
                    derivation.Validation.AssertNotExists(salesOrderItem, M.SalesOrderItem.ProductFeature);
                }

                if (salesOrderItem.ExistProduct && salesOrderItem.ExistQuantityOrdered && salesOrderItem.QuantityOrdered < salesOrderItem.QuantityShipped)
                {
                    derivation.Validation.AddError(salesOrderItem, M.SalesOrderItem.QuantityOrdered, ErrorMessages.SalesOrderItemLessThanAlreadeyShipped);
                }

                var isSubTotalItem = salesOrderItem.ExistInvoiceItemType && (salesOrderItem.InvoiceItemType.ProductItem || salesOrderItem.InvoiceItemType.PartItem);
                if (isSubTotalItem)
                {
                    if (salesOrderItem.QuantityOrdered == 0)
                    {
                        derivation.Validation.AddError(salesOrderItem, M.SalesOrderItem.QuantityOrdered, "QuantityOrdered is Required");
                    }
                }
                else
                {
                    if (salesOrderItem.AssignedUnitPrice == 0)
                    {
                        derivation.Validation.AddError(salesOrderItem, M.SalesOrderItem.AssignedUnitPrice, "Price is Required");
                    }
                }

                derivation.Validation.AssertExistsAtMostOne(salesOrderItem, M.SalesOrderItem.Product, M.SalesOrderItem.ProductFeature);
                derivation.Validation.AssertExistsAtMostOne(salesOrderItem, M.SalesOrderItem.SerialisedItem, M.SalesOrderItem.ProductFeature);
                derivation.Validation.AssertExistsAtMostOne(salesOrderItem, M.SalesOrderItem.ReservedFromSerialisedInventoryItem, M.SalesOrderItem.ReservedFromNonSerialisedInventoryItem);
                derivation.Validation.AssertExistsAtMostOne(salesOrderItem, M.SalesOrderItem.AssignedUnitPrice, M.SalesOrderItem.DiscountAdjustment, M.SalesOrderItem.SurchargeAdjustment);
            }

            var validOrderItems = this.SalesOrderItems.Where(v => v.IsValid).ToArray();
            this.ValidOrderItems = validOrderItems;

            this.SalesReps = validOrderItems
                .SelectMany(v => v.SalesReps)
                .Distinct()
                .ToArray();

            if (this.ExistVatRegime && this.VatRegime.ExistVatClause)
            {
                this.DerivedVatClause = this.VatRegime.VatClause;
            }
            else
            {
                string TakenbyCountry = null;

                if (this.TakenBy.PartyContactMechanisms?.FirstOrDefault(v => v.ContactPurposes.Any(p => Equals(p, new ContactMechanismPurposes(session).RegisteredOffice)))?.ContactMechanism is PostalAddress registeredOffice)
                {
                    TakenbyCountry = registeredOffice.Country.IsoCode;
                }

                var OutsideEUCustomer = this.BillToCustomer?.VatRegime?.Equals(new VatRegimes(session).Export);
                var shipFromBelgium = this.ValidOrderItems?.Cast<SalesOrderItem>().All(v => Equals("BE", v.ShipFromAddress?.Country?.IsoCode));
                var shipToEU = this.ValidOrderItems?.Cast<SalesOrderItem>().Any(v => Equals(true, v.ShipToAddress?.Country?.EuMemberState));
                var sellerResponsibleForTransport = this.SalesTerms.Any(v => Equals(v.TermType, new IncoTermTypes(session).Cif) || Equals(v.TermType, new IncoTermTypes(session).Cfr));
                var buyerResponsibleForTransport = this.SalesTerms.Any(v => Equals(v.TermType, new IncoTermTypes(session).Exw));

                if (Equals(this.VatRegime, new VatRegimes(session).ServiceB2B))
                {
                    this.DerivedVatClause = new VatClauses(session).ServiceB2B;
                }
                else if (Equals(this.VatRegime, new VatRegimes(session).IntraCommunautair))
                {
                    this.DerivedVatClause = new VatClauses(session).Intracommunautair;
                }
                else if (TakenbyCountry == "BE"
                         && OutsideEUCustomer.HasValue && OutsideEUCustomer.Value
                         && shipFromBelgium.HasValue && shipFromBelgium.Value
                         && shipToEU.HasValue && shipToEU.Value == false)
                {
                    if (sellerResponsibleForTransport)
                    {
                        // You sell goods to a customer out of the EU and the goods are being sold and transported from Belgium to another country out of the EU and you transport the goods and importer is the customer
                        this.DerivedVatClause = new VatClauses(session).BeArt39Par1Item1;
                    }
                    else if (buyerResponsibleForTransport)
                    {
                        // You sell goods to a customer out of the EU and the goods are being sold and transported from Belgium to another country out of the EU  and the customer does the transport of the goods and importer is the customer
                        this.DerivedVatClause = new VatClauses(session).BeArt39Par1Item2;
                    }
                }
            }

            this.DerivedVatClause = this.ExistAssignedVatClause ? this.AssignedVatClause : this.DerivedVatClause;

            var salesOrderShipmentStates = new SalesOrderShipmentStates(this.Strategy.Session);
            var salesOrderPaymentStates = new SalesOrderPaymentStates(this.Strategy.Session);
            var salesOrderInvoiceStates = new SalesOrderInvoiceStates(this.Strategy.Session);

            var salesOrderItemShipmentStates = new SalesOrderItemShipmentStates(derivation.Session);
            var salesOrderItemPaymentStates = new SalesOrderItemPaymentStates(derivation.Session);
            var salesOrderItemInvoiceStates = new SalesOrderItemInvoiceStates(derivation.Session);

            // SalesOrder Shipment State
            if (validOrderItems.Any())
            {
                if (validOrderItems.All(v => v.SalesOrderItemShipmentState.Shipped))
                {
                    this.SalesOrderShipmentState = salesOrderShipmentStates.Shipped;
                }
                else if (validOrderItems.All(v => v.SalesOrderItemShipmentState.NotShipped))
                {
                    this.SalesOrderShipmentState = salesOrderShipmentStates.NotShipped;
                }
                else if (validOrderItems.Any(v => v.SalesOrderItemShipmentState.InProgress))
                {
                    this.SalesOrderShipmentState = salesOrderShipmentStates.InProgress;
                }
                else
                {
                    this.SalesOrderShipmentState = salesOrderShipmentStates.PartiallyShipped;
                }

                // SalesOrder Payment State
                if (validOrderItems.All(v => v.SalesOrderItemPaymentState.Paid))
                {
                    this.SalesOrderPaymentState = salesOrderPaymentStates.Paid;
                }
                else if (validOrderItems.All(v => v.SalesOrderItemPaymentState.NotPaid))
                {
                    this.SalesOrderPaymentState = salesOrderPaymentStates.NotPaid;
                }
                else
                {
                    this.SalesOrderPaymentState = salesOrderPaymentStates.PartiallyPaid;
                }

                // SalesOrder Invoice State
                if (validOrderItems.All(v => v.SalesOrderItemInvoiceState.Invoiced))
                {
                    this.SalesOrderInvoiceState = salesOrderInvoiceStates.Invoiced;
                }
                else if (validOrderItems.All(v => v.SalesOrderItemInvoiceState.NotInvoiced))
                {
                    this.SalesOrderInvoiceState = salesOrderInvoiceStates.NotInvoiced;
                }
                else
                {
                    this.SalesOrderInvoiceState = salesOrderInvoiceStates.PartiallyInvoiced;
                }

                // SalesOrder OrderState
                if (this.SalesOrderShipmentState.Shipped && this.SalesOrderInvoiceState.Invoiced)
                {
                    this.SalesOrderState = new SalesOrderStates(this.Strategy.Session).Completed;
                }

                if (this.SalesOrderState.Completed && this.SalesOrderPaymentState.Paid)
                {
                    this.SalesOrderState = new SalesOrderStates(this.Strategy.Session).Finished;
                }
            }

            this.SetSalesOrderItemState();

            var currentPriceComponents = new PriceComponents(session).CurrentPriceComponents(this.OrderDate);

            var quantityOrderedByProduct = validOrderItems
                .Where(v => v.ExistProduct)
                .GroupBy(v => v.Product)
                .ToDictionary(v => v.Key, v => v.Sum(w => w.QuantityOrdered));

            // First run to calculate price
            foreach (var salesOrderItem in validOrderItems)
            {
                decimal quantityOrdered = 0;

                if (salesOrderItem.ExistProduct)
                {
                    quantityOrderedByProduct.TryGetValue(salesOrderItem.Product, out quantityOrdered);
                }

                foreach (SalesOrderItem featureItem in salesOrderItem.OrderedWithFeatures)
                {
                    featureItem.QuantityOrdered = salesOrderItem.QuantityOrdered;
                    this.CalculatePrices(derivation, featureItem, currentPriceComponents, quantityOrdered, 0);
                }

                this.CalculatePrices(derivation, salesOrderItem, currentPriceComponents, quantityOrdered, 0);
            }

            var totalBasePriceByProduct = validOrderItems
                .Where(v => v.ExistProduct)
                .GroupBy(v => v.Product)
                .ToDictionary(v => v.Key, v => v.Sum(w => w.TotalBasePrice));

            // Second run to calculate price (because of order value break)
            foreach (var salesOrderItem in validOrderItems)
            {
                decimal quantityOrdered = 0;
                decimal totalBasePrice = 0;

                if (salesOrderItem.ExistProduct)
                {
                    quantityOrderedByProduct.TryGetValue(salesOrderItem.Product, out quantityOrdered);
                    totalBasePriceByProduct.TryGetValue(salesOrderItem.Product, out totalBasePrice);
                }

                foreach (SalesOrderItem featureItem in salesOrderItem.OrderedWithFeatures)
                {
                    this.CalculatePrices(derivation, featureItem, currentPriceComponents, quantityOrdered, totalBasePrice);
                }

                this.CalculatePrices(derivation, salesOrderItem, currentPriceComponents, quantityOrdered, totalBasePrice);
            }

            // Calculate Totals
            if (this.ExistValidOrderItems)
            {
                this.TotalBasePrice = 0;
                this.TotalDiscount = 0;
                this.TotalSurcharge = 0;
                this.TotalExVat = 0;
                this.TotalFee = 0;
                this.TotalShippingAndHandling = 0;
                this.TotalVat = 0;
                this.TotalIncVat = 0;
                this.TotalListPrice = 0;

                foreach (var item in validOrderItems)
                {
                    if (!item.ExistSalesOrderItemWhereOrderedWithFeature)
                    {
                        this.TotalBasePrice += item.TotalBasePrice;
                        this.TotalDiscount += item.TotalDiscount;
                        this.TotalSurcharge += item.TotalSurcharge;
                        this.TotalExVat += item.TotalExVat;
                        this.TotalVat += item.TotalVat;
                        this.TotalIncVat += item.TotalIncVat;
                        this.TotalListPrice += item.UnitPrice;
                    }
                }

                if (this.ExistDiscountAdjustment)
                {
                    var discount = this.DiscountAdjustment.Percentage.HasValue ?
                                           Math.Round(this.TotalExVat * this.DiscountAdjustment.Percentage.Value / 100, 2) :
                                           this.DiscountAdjustment.Amount ?? 0;

                    this.TotalDiscount += discount;
                    this.TotalExVat -= discount;

                    if (this.ExistVatRegime)
                    {
                        var vat = Math.Round(discount * this.VatRegime.VatRate.Rate / 100, 2);

                        this.TotalVat -= vat;
                        this.TotalIncVat -= discount + vat;
                    }
                }

                if (this.ExistSurchargeAdjustment)
                {
                    var surcharge = this.SurchargeAdjustment.Percentage.HasValue ?
                                            Math.Round(this.TotalExVat * this.SurchargeAdjustment.Percentage.Value / 100, 2) :
                                            this.SurchargeAdjustment.Amount ?? 0;

                    this.TotalSurcharge += surcharge;
                    this.TotalExVat += surcharge;

                    if (this.ExistVatRegime)
                    {
                        var vat = Math.Round(surcharge * this.VatRegime.VatRate.Rate / 100, 2);
                        this.TotalVat += vat;
                        this.TotalIncVat += surcharge + vat;
                    }
                }

                if (this.ExistFee)
                {
                    var fee = this.Fee.Percentage.HasValue ?
                                      Math.Round(this.TotalExVat * this.Fee.Percentage.Value / 100, 2) :
                                      this.Fee.Amount ?? 0;

                    this.TotalFee += fee;
                    this.TotalExVat += fee;

                    if (this.Fee.ExistVatRate)
                    {
                        var vat1 = Math.Round(fee * this.Fee.VatRate.Rate / 100, 2);
                        this.TotalVat += vat1;
                        this.TotalIncVat += fee + vat1;
                    }
                }

                if (this.ExistShippingAndHandlingCharge)
                {
                    var shipping = this.ShippingAndHandlingCharge.Percentage.HasValue ?
                                           Math.Round(this.TotalExVat * this.ShippingAndHandlingCharge.Percentage.Value / 100, 2) :
                                           this.ShippingAndHandlingCharge.Amount ?? 0;

                    this.TotalShippingAndHandling += shipping;
                    this.TotalExVat += shipping;

                    if (this.ShippingAndHandlingCharge.ExistVatRate)
                    {
                        var vat2 = Math.Round(shipping * this.ShippingAndHandlingCharge.VatRate.Rate / 100, 2);
                        this.TotalVat += vat2;
                        this.TotalIncVat += shipping + vat2;
                    }
                }

                //// Only take into account items for which there is data at the item level.
                //// Skip negative sales.
                decimal totalUnitBasePrice = 0;
                decimal totalListPrice = 0;

                foreach (var item1 in validOrderItems)
                {
                    if (item1.TotalExVat > 0)
                    {
                        totalUnitBasePrice += item1.UnitBasePrice;
                        totalListPrice += item1.UnitPrice;
                    }
                }
            }

            // TODO: Move to versioning
            this.PreviousBillToCustomer = this.BillToCustomer;
            this.PreviousShipToCustomer = this.ShipToCustomer;

            var singleton = session.GetSingleton();

            this.AddSecurityToken(new SecurityTokens(session).DefaultSecurityToken);

            this.ResetPrintDocument();
        }

        private void SetSalesOrderItemState()
        {
            var validOrderItems = this.SalesOrderItems.Where(v => v.IsValid).ToArray();
            var salesOrderItemStates = new SalesOrderItemStates(this.Session());

            // SalesOrderItem States
            foreach (var salesOrderItem in validOrderItems)
            {
                if (this.SalesOrderState.readyForPosting &&
                    (salesOrderItem.SalesOrderItemState.Created || salesOrderItem.SalesOrderItemState.OnHold))
                {
                    salesOrderItem.SalesOrderItemState = salesOrderItemStates.ReadyForPosting;
                }

                if (this.SalesOrderState.InProcess && (salesOrderItem.SalesOrderItemState.ReadyForPosting || salesOrderItem.SalesOrderItemState.OnHold))
                {
                    salesOrderItem.SalesOrderItemState = salesOrderItemStates.InProcess;
                }

                if (this.SalesOrderState.OnHold && salesOrderItem.SalesOrderItemState.InProcess)
                {
                    salesOrderItem.SalesOrderItemState = salesOrderItemStates.OnHold;
                }

                if (this.SalesOrderState.Finished)
                {
                    salesOrderItem.SalesOrderItemState = salesOrderItemStates.Finished;
                }

                if (this.SalesOrderState.Cancelled)
                {
                    salesOrderItem.SalesOrderItemState = salesOrderItemStates.Cancelled;
                }

                if (this.SalesOrderState.Rejected)
                {
                    salesOrderItem.SalesOrderItemState = salesOrderItemStates.Rejected;
                }
            }
        }

        public void BaseOnPostDerive(ObjectOnPostDerive method)
        {
            var validOrderItems = this.SalesOrderItems.Where(v => v.IsValid).ToArray();

            // CanShip
            if (this.SalesOrderState.Equals(new SalesOrderStates(this.Strategy.Session).InProcess))
            {
                var somethingToShip = false;
                var allItemsAvailable = true;

                foreach (var salesOrderItem1 in validOrderItems)
                {
                    if (!this.PartiallyShip && salesOrderItem1.QuantityRequestsShipping != salesOrderItem1.QuantityOrdered)
                    {
                        allItemsAvailable = false;
                        break;
                    }

                    if (this.PartiallyShip && salesOrderItem1.QuantityRequestsShipping > 0)
                    {
                        somethingToShip = true;
                    }
                }

                this.CanShip = (!this.PartiallyShip && allItemsAvailable) || somethingToShip;
            }
            else
            {
                this.CanShip = false;
            }

            // CanInvoice
            if (this.SalesOrderState.InProcess && object.Equals(this.Store.BillingProcess, new BillingProcesses(this.Strategy.Session).BillingForOrderItems))
            {
                this.CanInvoice = false;

                foreach (var orderItem2 in validOrderItems)
                {
                    var amountAlreadyInvoiced1 = orderItem2.OrderItemBillingsWhereOrderItem.Sum(v => v.Amount);

                    var leftToInvoice1 = (orderItem2.QuantityOrdered * orderItem2.UnitPrice) - amountAlreadyInvoiced1;

                    if (leftToInvoice1 > 0)
                    {
                        this.CanInvoice = true;
                    }
                }
            }
            else
            {
                this.CanInvoice = false;
            }

            if (this.SalesOrderState.Equals(new SalesOrderStates(this.Strategy.Session).InProcess) &&
                Equals(this.Store.BillingProcess, new BillingProcesses(this.Strategy.Session).BillingForShipmentItems))
            {
                this.RemoveDeniedPermission(new Permissions(this.Strategy.Session).Get(this.Meta.Class, this.Meta.Invoice, Operations.Execute));
            }

            foreach (SalesOrderItem salesOrderItem in this.ValidOrderItems)
            {
                if (salesOrderItem.ExistReservedFromNonSerialisedInventoryItem)
                {
                    if (!salesOrderItem.ReservedFromNonSerialisedInventoryItem.Equals(salesOrderItem.PreviousReservedFromNonSerialisedInventoryItem))
                    {
                        method.Derivation.Cycle.Iteration.AddDependency(salesOrderItem.PreviousReservedFromNonSerialisedInventoryItem, this);
                    }
                }
            }

            if (this.CanShip && this.Store.AutoGenerateCustomerShipment)
            {
                this.Ship();
                this.AddDeniedPermission(new Permissions(this.Strategy.Session).Get(this.Meta.Class, this.Meta.Ship, Operations.Execute));
            }

            if (!this.CanShip)
            {
                this.AddDeniedPermission(new Permissions(this.Strategy.Session).Get(this.Meta.Class, this.Meta.Ship, Operations.Execute));
            }

            if (!this.CanInvoice)
            {
                this.AddDeniedPermission(new Permissions(this.Strategy.Session).Get(this.Meta.Class, this.Meta.Invoice, Operations.Execute));
            }
        }

        public void BaseCancel(OrderCancel method)
        {
            this.OnCancelOrReject();
            this.SalesOrderState = new SalesOrderStates(this.Strategy.Session).Cancelled;
            this.SetSalesOrderItemState();
        }

        public void BaseConfirm(OrderConfirm method)
        {
            var orderThreshold = this.Store.OrderThreshold;
            var partyFinancial = this.BillToCustomer.PartyFinancialRelationshipsWhereParty.FirstOrDefault(v => Equals(v.InternalOrganisation, this.TakenBy));

            var amountOverDue = partyFinancial.AmountOverDue;
            var creditLimit = partyFinancial.CreditLimit ?? (this.Store.ExistCreditLimit ? this.Store.CreditLimit : 0);

            if (amountOverDue > creditLimit || this.TotalExVat < orderThreshold)
            {
                this.SalesOrderState = new SalesOrderStates(this.Strategy.Session).RequestsApproval;
            }
            else
            {
                this.SalesOrderState = new SalesOrderStates(this.Strategy.Session).ReadyForPosting;
            }

            this.SetSalesOrderItemState();
        }

        public void BaseReject(OrderReject method)
        {
            this.OnCancelOrReject();
            this.SalesOrderState = new SalesOrderStates(this.Strategy.Session).Rejected;
            this.SetSalesOrderItemState();
        }

        public void BaseHold(OrderHold method)
        {
            this.SalesOrderState = new SalesOrderStates(this.Strategy.Session).OnHold;
            this.SetSalesOrderItemState();
        }

        public void BaseApprove(OrderApprove method)
        {
            this.SalesOrderState = new SalesOrderStates(this.Strategy.Session).ReadyForPosting;
            this.SetSalesOrderItemState();
        }

        public void BaseSend(SalesOrderSend method)
        {
            this.SalesOrderState = new SalesOrderStates(this.Strategy.Session).InProcess;
            this.SetSalesOrderItemState();
        }

        public void BaseContinue(OrderContinue method)
        {
            this.SalesOrderState = this.PreviousSalesOrderState;
            this.SetSalesOrderItemState();
        }

        public void BaseComplete(OrderComplete method)
        {
            this.SalesOrderState = new SalesOrderStates(this.Strategy.Session).Completed;
            this.SetSalesOrderItemState();
        }

        public void BaseShip(SalesOrderShip method)
        {
            if (this.CanShip)
            {
                var addresses = this.ShipToAddresses();
                var shipments = new List<Shipment>();
                if (addresses.Count > 0)
                {
                    foreach (var address in addresses)
                    {
                        var pendingShipment = address.Value.BaseGetPendingCustomerShipmentForStore(address.Key, this.Store, this.ShipmentMethod);

                        if (pendingShipment == null)
                        {
                            pendingShipment = new CustomerShipmentBuilder(this.Strategy.Session)
                                .WithShipFromParty(this.TakenBy)
                                .WithShipFromAddress(this.ShipFromAddress)
                                .WithShipToAddress(address.Key)
                                .WithShipToParty(address.Value)
                                .WithStore(this.Store)
                                .WithShipmentMethod(this.ShipmentMethod)
                                .WithPaymentMethod(this.PaymentMethod)
                                .Build();

                            if (this.Store.AutoGenerateShipmentPackage)
                            {
                                pendingShipment.AddShipmentPackage(new ShipmentPackageBuilder(this.Strategy.Session).Build());
                            }
                        }

                        foreach (SalesOrderItem orderItem in this.ValidOrderItems)
                        {
                            var orderItemDerivedRoles = (SalesOrderItemDerivedRoles)orderItem;

                            if (orderItem.ExistProduct && orderItem.ShipToAddress.Equals(address.Key) && orderItem.QuantityRequestsShipping > 0)
                            {
                                var good = orderItem.Product as Good;
                                var nonUnifiedGood = orderItem.Product as NonUnifiedGood;
                                var inventoryItemKind = orderItem.Product is UnifiedGood unifiedGood ? unifiedGood.InventoryItemKind : nonUnifiedGood?.Part.InventoryItemKind;

                                ShipmentItem shipmentItem = null;
                                foreach (ShipmentItem item in pendingShipment.ShipmentItems)
                                {
                                    if (inventoryItemKind != null
                                        && inventoryItemKind.Equals(new InventoryItemKinds(this.Session()).NonSerialised)
                                        && item.Good.Equals(good)
                                        && !item.ItemIssuancesWhereShipmentItem.Any(v => v.PickListItem.PickListWherePickListItem.PickListState.Equals(new PickListStates(this.Session()).Picked)))
                                    {
                                        shipmentItem = item;
                                        break;
                                    }
                                }

                                if (shipmentItem != null)
                                {
                                    shipmentItem.Quantity += orderItem.QuantityRequestsShipping;
                                    shipmentItem.ContentsDescription = $"{shipmentItem.Quantity} * {good.Name}";
                                }
                                else
                                {
                                    shipmentItem = new ShipmentItemBuilder(this.Strategy.Session)
                                        .WithGood(good)
                                        .WithQuantity(orderItem.QuantityRequestsShipping)
                                        .WithContentsDescription($"{orderItem.QuantityRequestsShipping} * {good}")
                                        .Build();

                                    if (orderItem.ExistSerialisedItem)
                                    {
                                        shipmentItem.SerialisedItem = orderItem.SerialisedItem;
                                    }

                                    if (orderItem.ExistNewSerialisedItemState)
                                    {
                                        shipmentItem.NewSerialisedItemState = orderItem.NewSerialisedItemState;
                                    }

                                    if (orderItem.ExistReservedFromNonSerialisedInventoryItem)
                                    {
                                        shipmentItem.AddReservedFromInventoryItem(orderItem.ReservedFromNonSerialisedInventoryItem);
                                    }

                                    if (orderItem.ExistReservedFromSerialisedInventoryItem)
                                    {
                                        shipmentItem.AddReservedFromInventoryItem(orderItem.ReservedFromSerialisedInventoryItem);
                                    }

                                    pendingShipment.AddShipmentItem(shipmentItem);
                                }

                                foreach (SalesOrderItem featureItem in orderItem.OrderedWithFeatures)
                                {
                                    shipmentItem.AddProductFeature(featureItem.ProductFeature);
                                }

                                new OrderShipmentBuilder(this.Strategy.Session)
                                    .WithOrderItem(orderItem)
                                    .WithShipmentItem(shipmentItem)
                                    .WithQuantity(orderItem.QuantityRequestsShipping)
                                    .Build();

                                orderItemDerivedRoles.QuantityRequestsShipping = 0;
                            }
                        }

                        shipments.Add(pendingShipment);
                        this.AddDeniedPermission(new Permissions(this.Strategy.Session).Get(this.Meta.Class, this.Meta.Ship, Operations.Execute));
                    }
                }
            }
        }

        public void BaseInvoice(SalesOrderInvoice method)
        {
            if (this.CanInvoice)
            {
                var salesInvoice = new SalesInvoiceBuilder(this.Strategy.Session)
                    .WithBilledFrom(this.TakenBy)
                    .WithBilledFromContactMechanism(this.TakenByContactMechanism)
                    .WithBilledFromContactPerson(this.TakenByContactPerson)
                    .WithBillToCustomer(this.BillToCustomer)
                    .WithBillToContactMechanism(this.BillToContactMechanism)
                    .WithBillToContactPerson(this.BillToContactPerson)
                    .WithBillToEndCustomer(this.BillToEndCustomer)
                    .WithBillToEndCustomerContactMechanism(this.BillToEndCustomerContactMechanism)
                    .WithBillToEndCustomerContactPerson(this.BillToEndCustomerContactPerson)
                    .WithShipToCustomer(this.ShipToCustomer)
                    .WithShipToAddress(this.ShipToAddress)
                    .WithShipToContactPerson(this.ShipToContactPerson)
                    .WithShipToEndCustomer(this.ShipToEndCustomer)
                    .WithShipToEndCustomerAddress(this.ShipToEndCustomerAddress)
                    .WithShipToEndCustomerContactPerson(this.ShipToEndCustomerContactPerson)
                    .WithDescription(this.Description)
                    .WithStore(this.Store)
                    .WithInvoiceDate(this.Session().Now())
                    .WithSalesChannel(this.SalesChannel)
                    .WithSalesInvoiceType(new SalesInvoiceTypes(this.Strategy.Session).SalesInvoice)
                    .WithVatRegime(this.VatRegime)
                    .WithAssignedVatClause(this.DerivedVatClause)
                    .WithDiscountAdjustment(this.DiscountAdjustment)
                    .WithSurchargeAdjustment(this.SurchargeAdjustment)
                    .WithShippingAndHandlingCharge(this.ShippingAndHandlingCharge)
                    .WithFee(this.Fee)
                    .WithCustomerReference(this.CustomerReference)
                    .WithPaymentMethod(this.PaymentMethod)
                    .Build();

                foreach (SalesOrderItem orderItem in this.ValidOrderItems)
                {
                    var amountAlreadyInvoiced = orderItem.OrderItemBillingsWhereOrderItem.Sum(v => v.Amount);

                    var leftToInvoice = (orderItem.QuantityOrdered * orderItem.UnitPrice) - amountAlreadyInvoiced;

                    if (leftToInvoice > 0)
                    {
                        var invoiceItem = new SalesInvoiceItemBuilder(this.Strategy.Session)
                            .WithInvoiceItemType(orderItem.InvoiceItemType)
                            .WithAssignedUnitPrice(orderItem.UnitPrice)
                            .WithProduct(orderItem.Product)
                            .WithQuantity(orderItem.QuantityOrdered)
                            .WithAssignedVatRegime(orderItem.AssignedVatRegime)
                            .WithDescription(orderItem.Description)
                            .WithInternalComment(orderItem.InternalComment)
                            .WithMessage(orderItem.Message)
                            .Build();

                        if (orderItem.ExistSerialisedItem)
                        {
                            invoiceItem.SerialisedItem = orderItem.SerialisedItem;
                        }

                        salesInvoice.AddSalesInvoiceItem(invoiceItem);

                        new OrderItemBillingBuilder(this.Strategy.Session)
                            .WithQuantity(orderItem.QuantityOrdered)
                            .WithAmount(leftToInvoice)
                            .WithOrderItem(orderItem)
                            .WithInvoiceItem(invoiceItem)
                            .Build();
                    }
                }

                foreach (SalesTerm salesTerm in this.SalesTerms)
                {
                    if (salesTerm.GetType().Name == typeof(IncoTerm).Name)
                    {
                        salesInvoice.AddSalesTerm(new IncoTermBuilder(this.Strategy.Session)
                            .WithTermType(salesTerm.TermType)
                            .WithTermValue(salesTerm.TermValue)
                            .WithDescription(salesTerm.Description)
                            .Build());
                    }

                    if (salesTerm.GetType().Name == typeof(InvoiceTerm).Name)
                    {
                        salesInvoice.AddSalesTerm(new InvoiceTermBuilder(this.Strategy.Session)
                            .WithTermType(salesTerm.TermType)
                            .WithTermValue(salesTerm.TermValue)
                            .WithDescription(salesTerm.Description)
                            .Build());
                    }

                    if (salesTerm.GetType().Name == typeof(OrderTerm).Name)
                    {
                        salesInvoice.AddSalesTerm(new OrderTermBuilder(this.Strategy.Session)
                            .WithTermType(salesTerm.TermType)
                            .WithTermValue(salesTerm.TermValue)
                            .WithDescription(salesTerm.Description)
                            .Build());
                    }
                }
            }
        }

        public void BasePrint(PrintablePrint method)
        {
            if (!method.IsPrinted)
            {
                var singleton = this.Strategy.Session.GetSingleton();
                var logo = this.TakenBy?.ExistLogoImage == true ?
                               this.TakenBy.LogoImage.MediaContent.Data :
                               singleton.LogoImage.MediaContent.Data;

                var images = new Dictionary<string, byte[]>
                                 {
                                     { "Logo", logo },
                                 };

                if (this.ExistOrderNumber)
                {
                    var session = this.Strategy.Session;
                    var barcodeService = session.ServiceProvider.GetRequiredService<IBarcodeService>();
                    var barcode = barcodeService.Generate(this.OrderNumber, BarcodeType.CODE_128, 320, 80);
                    images.Add("Barcode", barcode);
                }

                var model = new Print.SalesOrderModel.Model(this);
                this.RenderPrintDocument(this.TakenBy?.SalesOrderTemplate, model, images);

                this.PrintDocument.Media.FileName = $"{this.OrderNumber}.odt";
            }
        }

        public void CalculatePrices(
            IDerivation derivation,
            SalesOrderItem salesOrderItem,
            PriceComponent[] currentPriceComponents,
            decimal quantityOrdered,
            decimal totalBasePrice)
        {
            var salesOrderItemDerivedRoles = (SalesOrderItemDerivedRoles)salesOrderItem;

            var currentGenericOrProductOrFeaturePriceComponents = Array.Empty<PriceComponent>();
            if (salesOrderItem.ExistProduct)
            {
                currentGenericOrProductOrFeaturePriceComponents = salesOrderItem.Product.GetPriceComponents(currentPriceComponents);
            }
            else if (salesOrderItem.ExistProductFeature)
            {
                currentGenericOrProductOrFeaturePriceComponents = salesOrderItem.ProductFeature.GetPriceComponents(salesOrderItem.SalesOrderItemWhereOrderedWithFeature.Product, currentPriceComponents);
            }

            var priceComponents = currentGenericOrProductOrFeaturePriceComponents.Where(
                v => PriceComponents.BaseIsApplicable(
                    new PriceComponents.IsApplicable
                    {
                        PriceComponent = v,
                        Customer = this.BillToCustomer,
                        Product = salesOrderItem.Product,
                        SalesOrder = this,
                        QuantityOrdered = quantityOrdered,
                        ValueOrdered = totalBasePrice,
                    })).ToArray();

            var unitBasePrice = priceComponents.OfType<BasePrice>().Min(v => v.Price);

            // Calculate Unit Price (with Discounts and Surcharges)
            if (salesOrderItem.AssignedUnitPrice.HasValue)
            {
                salesOrderItemDerivedRoles.UnitBasePrice = unitBasePrice ?? salesOrderItem.AssignedUnitPrice.Value;
                salesOrderItemDerivedRoles.UnitDiscount = 0;
                salesOrderItemDerivedRoles.UnitSurcharge = 0;
                salesOrderItemDerivedRoles.UnitPrice = salesOrderItem.AssignedUnitPrice.Value;
            }
            else
            {
                if (!unitBasePrice.HasValue)
                {
                    derivation.Validation.AddError(salesOrderItem, M.SalesOrderItem.UnitBasePrice, "No BasePrice with a Price");
                    return;
                }

                salesOrderItemDerivedRoles.UnitBasePrice = unitBasePrice.Value;

                salesOrderItemDerivedRoles.UnitDiscount = priceComponents.OfType<DiscountComponent>().Sum(
                    v => v.Percentage.HasValue
                             ? Math.Round(salesOrderItem.UnitBasePrice * v.Percentage.Value / 100, 2)
                             : v.Price ?? 0);

                salesOrderItemDerivedRoles.UnitSurcharge = priceComponents.OfType<SurchargeComponent>().Sum(
                    v => v.Percentage.HasValue
                             ? Math.Round(salesOrderItem.UnitBasePrice * v.Percentage.Value / 100, 2)
                             : v.Price ?? 0);

                salesOrderItemDerivedRoles.UnitPrice = salesOrderItem.UnitBasePrice - salesOrderItem.UnitDiscount + salesOrderItem.UnitSurcharge;

                if (salesOrderItem.ExistDiscountAdjustment)
                {
                    salesOrderItemDerivedRoles.UnitDiscount += salesOrderItem.DiscountAdjustment.Percentage.HasValue ?
                        Math.Round(salesOrderItem.UnitPrice * salesOrderItem.DiscountAdjustment.Percentage.Value / 100, 2) :
                        salesOrderItem.DiscountAdjustment.Amount ?? 0;
                }

                if (salesOrderItem.ExistSurchargeAdjustment)
                {
                    salesOrderItemDerivedRoles.UnitSurcharge += salesOrderItem.SurchargeAdjustment.Percentage.HasValue ?
                        Math.Round(salesOrderItem.UnitPrice * salesOrderItem.SurchargeAdjustment.Percentage.Value / 100, 2) :
                        salesOrderItem.SurchargeAdjustment.Amount ?? 0;
                }

                salesOrderItemDerivedRoles.UnitPrice = salesOrderItem.UnitBasePrice - salesOrderItem.UnitDiscount + salesOrderItem.UnitSurcharge;
            }

            foreach (SalesOrderItem featureItem in salesOrderItem.OrderedWithFeatures)
            {
                salesOrderItemDerivedRoles.UnitBasePrice += featureItem.UnitBasePrice;
                salesOrderItemDerivedRoles.UnitPrice += featureItem.UnitPrice;
                salesOrderItemDerivedRoles.UnitDiscount += featureItem.UnitDiscount;
                salesOrderItemDerivedRoles.UnitSurcharge += featureItem.UnitSurcharge;
            }

            salesOrderItemDerivedRoles.UnitVat = salesOrderItem.ExistVatRate ? Math.Round(salesOrderItem.UnitPrice * salesOrderItem.VatRate.Rate / 100, 2) : 0;

            // Calculate Totals
            salesOrderItemDerivedRoles.TotalBasePrice = salesOrderItem.UnitBasePrice * salesOrderItem.QuantityOrdered;
            salesOrderItemDerivedRoles.TotalDiscount = salesOrderItem.UnitDiscount * salesOrderItem.QuantityOrdered;
            salesOrderItemDerivedRoles.TotalSurcharge = salesOrderItem.UnitSurcharge * salesOrderItem.QuantityOrdered;
            salesOrderItemDerivedRoles.TotalOrderAdjustment = salesOrderItem.TotalSurcharge - salesOrderItem.TotalDiscount;

            if (salesOrderItem.TotalBasePrice > 0)
            {
                salesOrderItemDerivedRoles.TotalDiscountAsPercentage = Math.Round(salesOrderItem.TotalDiscount / salesOrderItem.TotalBasePrice * 100, 2);
                salesOrderItemDerivedRoles.TotalSurchargeAsPercentage = Math.Round(salesOrderItem.TotalSurcharge / salesOrderItem.TotalBasePrice * 100, 2);
            }
            else
            {
                salesOrderItemDerivedRoles.TotalDiscountAsPercentage = 0;
                salesOrderItemDerivedRoles.TotalSurchargeAsPercentage = 0;
            }

            salesOrderItemDerivedRoles.TotalExVat = salesOrderItem.UnitPrice * salesOrderItem.QuantityOrdered;
            salesOrderItemDerivedRoles.TotalVat = salesOrderItem.UnitVat * salesOrderItem.QuantityOrdered;
            salesOrderItemDerivedRoles.TotalIncVat = salesOrderItem.TotalExVat + salesOrderItem.TotalVat;
        }

        private Dictionary<PostalAddress, Party> ShipToAddresses()
        {
            var addresses = new Dictionary<PostalAddress, Party>();
            foreach (SalesOrderItem item in this.ValidOrderItems)
            {
                if (item.QuantityRequestsShipping > 0)
                {
                    if (!addresses.ContainsKey(item.ShipToAddress))
                    {
                        addresses.Add(item.ShipToAddress, item.ShipToParty);
                    }
                }
            }

            return addresses;
        }

        private void OnCancelOrReject()
        {
            foreach (SalesOrderItem salesOrderItem in this.ValidOrderItems)
            {
                salesOrderItem.Cancel();
            }
        }
    }
}
