// <copyright file="PurchaseOrders.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using Allors.Meta;

    public partial class PurchaseOrders
    {
        protected override void BasePrepare(Setup setup) => setup.AddDependency(this.ObjectType, M.PurchaseOrderState);

        protected override void BaseSecure(Security config)
        {
            var states = new PurchaseOrderStates(this.Session);
            var created = states.Created;
            var onHold = states.OnHold;
            var cancelled = states.Cancelled;
            var rejected = states.Rejected;
            var awaitingApprovalLevel1 = states.AwaitingApprovalLevel1;
            var awaitingApprovalLevel2 = states.AwaitingApprovalLevel2;
            var inProcess = states.InProcess;
            var sent = states.Sent;
            var completed = states.Completed;
            var finished = states.Finished;

            var shipmentStates = new PurchaseOrderShipmentStates(this.Session);
            var partiallyReceived = shipmentStates.PartiallyReceived;
            var received = shipmentStates.Received;

            var approve = this.Meta.Approve;
            var reject = this.Meta.Reject;
            var hold = this.Meta.Hold;
            var @continue = this.Meta.Continue;
            var confirm = this.Meta.Confirm;
            var cancel = this.Meta.Cancel;
            var reopen = this.Meta.Reopen;
            var send = this.Meta.Send;
            var quickReceive = this.Meta.QuickReceive;
            var invoice = this.Meta.Invoice;

            config.Deny(this.ObjectType, created, approve, reject, @continue, reopen, send, quickReceive, invoice);
            config.Deny(this.ObjectType, onHold, approve, hold, confirm, reopen, send, quickReceive, invoice);
            config.Deny(this.ObjectType, cancelled, approve, reject, hold, @continue, confirm, cancel, send, quickReceive, invoice);
            config.Deny(this.ObjectType, rejected, approve, reject, hold, @continue, confirm, cancel, send, quickReceive, invoice);
            config.Deny(this.ObjectType, awaitingApprovalLevel1, hold, @continue, confirm, cancel, reopen, send, quickReceive, @continue);
            config.Deny(this.ObjectType, awaitingApprovalLevel2, hold, @continue, confirm, cancel, reopen, send, quickReceive, @continue);
            config.Deny(this.ObjectType, inProcess, approve, reject, @continue, confirm, quickReceive);
            config.Deny(this.ObjectType, sent, approve, reject, hold, @continue, confirm, reopen, send);
            config.Deny(this.ObjectType, partiallyReceived, cancel, reject);
            config.Deny(this.ObjectType, received, cancel, reject, quickReceive);
            config.Deny(this.ObjectType, completed, approve, reject, hold, @continue, confirm, cancel, reopen, send, quickReceive);

            config.Deny(this.ObjectType, inProcess, Operations.Write);
            config.Deny(this.ObjectType, cancelled, Operations.Write);
            config.Deny(this.ObjectType, rejected, Operations.Write, Operations.Execute);
            config.Deny(this.ObjectType, completed, Operations.Write);
            config.Deny(this.ObjectType, finished, Operations.Execute, Operations.Write);
        }
    }
}
