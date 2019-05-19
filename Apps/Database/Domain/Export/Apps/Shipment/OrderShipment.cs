// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrderShipment.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;
using Allors.Domain.NonLogging;
using Resources;

namespace Allors.Domain
{
    using Meta;

    public partial class OrderShipment
    {
        public void AppsOnPreDerive(ObjectOnPreDerive method)
        {
            var derivation = method.Derivation;

            if (derivation.IsModified(this))
            {
                derivation.AddDependency(this.OrderItem, this);
            }
        }

        public void AppsOnDerive(ObjectOnDerive method)
        {
            var derivation = method.Derivation;

            var salesOrderItem = this.OrderItem as SalesOrderItem;

            if (this.ShipmentItem.ShipmentWhereShipmentItem is CustomerShipment customerShipment && salesOrderItem != null)
            {
                var quantityPendingShipment = this.OrderItem?.OrderShipmentsWhereOrderItem?.Where(v => v.ExistShipmentItem && !((CustomerShipment)v.ShipmentItem.ShipmentWhereShipmentItem).ShipmentState.Equals(new ShipmentStates(this.strategy.Session).Shipped)).Sum(v => v.Quantity);

                if (salesOrderItem.QuantityPendingShipment > quantityPendingShipment)
                {
                    var diff = this.Quantity * -1;
                    salesOrderItem.QuantityPendingShipment -= diff;
                    customerShipment.AppsOnDeriveQuantityDecreased(this.ShipmentItem, salesOrderItem, diff);
                }

                if (this.strategy.IsNewInSession)
                {
                    var quantityPicked = this.OrderItem.OrderShipmentsWhereOrderItem.Select(v => v.ShipmentItem?.ItemIssuancesWhereShipmentItem.Sum(z => z.PickListItem.Quantity)).Sum();

                    if (salesOrderItem.ExistReservedFromNonSerialisedInventoryItem && this.Quantity > salesOrderItem.ReservedFromNonSerialisedInventoryItem.QuantityOnHand + quantityPicked)
                    {
                        derivation.Validation.AddError(this, M.OrderShipment.Quantity, ErrorMessages.SalesOrderItemQuantityToShipNowNotAvailable);
                    }
                    else if (this.Quantity > salesOrderItem.QuantityOrdered)
                    {
                        derivation.Validation.AddError(this, M.OrderShipment.Quantity, ErrorMessages.SalesOrderItemQuantityToShipNowIsLargerThanQuantityOrdered);
                    }
                    else
                    {
                        if (this.Quantity > salesOrderItem.QuantityOrdered - salesOrderItem.QuantityShipped - salesOrderItem.QuantityPendingShipment + salesOrderItem.QuantityReturned + quantityPicked)
                        {
                            derivation.Validation.AddError(this, M.OrderShipment.Quantity, ErrorMessages.SalesOrderItemQuantityToShipNowIsLargerThanQuantityRemaining);
                        }
                    }
                }
            }
        }
    }
}