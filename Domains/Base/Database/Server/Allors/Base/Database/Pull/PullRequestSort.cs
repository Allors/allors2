﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PullRequestSort.cs" company="Allors bvba">
//   Copyright 2002-2017 Allors bvba.
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

namespace Allors.Server
{
    using System;

    using Allors.Meta;

    public class PullRequestSort
    {
        /// <summary>
        /// The RoleType.
        /// </summary>
        public string RT { get; set; }

        /// <summary>
        /// The TreeNodes
        /// </summary>
        public string D { get; set; }

        public Sort Parse(MetaPopulation metaPopulation)
        {
            var roleType = (RoleType)metaPopulation.Find(new Guid(this.RT));

            return new Sort
                       {
                           RoleType = roleType,
                           Direction = "DESC".Equals(this.D?.ToUpper()) ? SortDirection.Descending : SortDirection.Ascending
                       };
        }
    }
}