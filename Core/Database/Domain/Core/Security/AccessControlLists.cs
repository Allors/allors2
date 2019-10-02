// <copyright file="AccessControlListFactory.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System.Collections.Generic;
    using System.Linq;

    public class AccessControlLists : IAccessControlLists
    {
        public AccessControlLists(User user)
        {
            this.User = user;
            this.EffectivePermissionIdsByAccessControl = user.EffectivePermissionsByAccessControl();
            this.AclByObject = new Dictionary<IObject, IAccessControlList>();
        }

        public IEnumerable<AccessControl> AccessControls => this.AclByObject.SelectMany(v => v.Value.AccessControls).Distinct();

        public IReadOnlyDictionary<AccessControl, HashSet<long>> EffectivePermissionIdsByAccessControl { get; set; }

        public User User { get; }

        private Dictionary<IObject, IAccessControlList> AclByObject { get; }

        public IAccessControlList this[IObject @object]
        {
            get
            {
                if (!this.AclByObject.TryGetValue(@object, out var acl))
                {
                    acl = new AccessControlList(this, @object);
                    this.AclByObject.Add(@object, acl);
                }

                return acl;
            }
        }
    }
}