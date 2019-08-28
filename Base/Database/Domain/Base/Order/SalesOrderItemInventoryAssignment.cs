// <copyright file="SalesOrderItemInventoryAssignment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System.Linq;

    public partial class SalesOrderItemInventoryAssignment
    {
        public void BaseOnPreDerive(ObjectOnPreDerive method)
        {
            var derivation = method.Derivation;

            derivation.AddDependency(this.InventoryItem, this);
            derivation.AddDependency(this.SalesOrderItem, this);
        }

        public void BaseOnDerive(ObjectOnDerive method)
        {
            var state = this.SalesOrderItem.SalesOrderItemState;
            var inventoryItemChanged = this.ExistCurrentVersion && (!Equals(this.CurrentVersion.InventoryItem, this.InventoryItem));

            foreach (InventoryTransactionReason createReason in state.InventoryTransactionReasonsToCreate)
            {
                this.SyncInventoryTransactions(this.InventoryItem, this.Quantity, createReason, false);
            }

            foreach (InventoryTransactionReason cancelReason in state.InventoryTransactionReasonsToCancel)
            {
                this.SyncInventoryTransactions(this.InventoryItem, this.Quantity, cancelReason, true);
            }

            if (inventoryItemChanged)
            {
                // CurrentVersion is Previous Version until PostDerive
                var previousInventoryItem = this.CurrentVersion.InventoryItem;
                var previousQuantity = this.CurrentVersion.Quantity;
                state = this.CurrentVersion.SalesOrderItem.PreviousSalesOrderItemState ?? this.CurrentVersion.SalesOrderItem.SalesOrderItemState;

                foreach (InventoryTransactionReason createReason in state.InventoryTransactionReasonsToCreate)
                {
                    this.SyncInventoryTransactions(previousInventoryItem, previousQuantity, createReason, true);
                }

                foreach (InventoryTransactionReason cancelReason in state.InventoryTransactionReasonsToCancel)
                {
                    this.SyncInventoryTransactions(previousInventoryItem, previousQuantity, cancelReason, true);
                }
            }
        }

        private void SyncInventoryTransactions(InventoryItem inventoryItem, decimal initialQuantity, InventoryTransactionReason reason, bool isCancellation)
        {
            var adjustmentQuantity = 0M;
            var existingQuantity = 0M;
            var matchingTransactions = this.InventoryItemTransactions.Where(t => t.Reason.Equals(reason) && t.Part.Equals(inventoryItem.Part) && t.InventoryItem.Equals(inventoryItem)).ToArray();

            if (matchingTransactions.Length > 0)
            {
                existingQuantity = matchingTransactions.Sum(t => t.Quantity);
            }

            if (isCancellation)
            {
                adjustmentQuantity = 0 - existingQuantity;
            }
            else
            {
                adjustmentQuantity = initialQuantity - existingQuantity;
            }

            if (adjustmentQuantity != 0)
            {
                var newTransaction = new InventoryItemTransactionBuilder(this.strategy.Session)
                    .WithPart(inventoryItem.Part)
                    .WithQuantity(adjustmentQuantity)
                    .WithReason(reason)
                    .WithFacility(inventoryItem.Facility)
                    .Build();

                newTransaction.InventoryItem = inventoryItem;
                this.AddInventoryItemTransaction(newTransaction);
            }
        }
    }
}