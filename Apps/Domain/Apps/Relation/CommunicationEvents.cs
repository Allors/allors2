// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommunicationEvents.cs" company="Allors bvba">
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
    using Meta;

    public partial class CommunicationEvents
    {
        protected override void AppsPrepare(Setup setup)
        {
            base.AppsPrepare(setup);

            setup.AddDependency(this.ObjectType, M.CommunicationEventObjectState);
        }

        protected override void AppsSecure(Security config)
        {
            base.AppsSecure(config);

            var full = new[] { Operations.Read, Operations.Write, Operations.Execute };

            config.GrantAdministrator(this.ObjectType, full);
            config.GrantOwner(this.ObjectType, full);

            ObjectState scheduled = new CommunicationEventObjectStates(this.Session).Scheduled;
            ObjectState cancelled = new CommunicationEventObjectStates(this.Session).Cancelled;
            ObjectState closed = new CommunicationEventObjectStates(this.Session).Completed;

            var reopenId = M.CommunicationEvent.Reopen;
            var closeId = M.CommunicationEvent.Close;
            var cancelId = M.CommunicationEvent.Cancel;

            config.Deny(this.ObjectType, scheduled, reopenId);
            config.Deny(this.ObjectType, closed, closeId, cancelId);

            config.Deny(this.ObjectType, closed, Operations.Write);
            config.Deny(this.ObjectType, cancelled, Operations.Execute, Operations.Write);
        }
    }
}