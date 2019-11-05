// <copyright file="TimeSheet.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;

    public partial class TimeSheet
    {
        public void BaseOnPreDerive(ObjectOnPreDerive method)
        {
            var derivation = method.Derivation;

            if (derivation.HasChangedRole(this, this.Meta.TimeEntries))
            {
                foreach (TimeEntry timeEntry in this.TimeEntries)
                {
                    derivation.AddDependency(timeEntry, this);
                }
            }
        }

        public void BaseDelegateAccess(DelegatedAccessControlledObjectDelegateAccess method)
        {
            var securityTokens = new[] { this.Session().GetSingleton().DefaultSecurityToken, this.Worker.OwnerSecurityToken };
            method.SecurityTokens = securityTokens;
        }

        public void BaseDelete(DeletableDelete method)
        {
            if (this.ExistTimeEntries)
            {
                throw new Exception("Cannot delete TimeSheet due to associated TimeEntry details");
            }
        }
    }
}
