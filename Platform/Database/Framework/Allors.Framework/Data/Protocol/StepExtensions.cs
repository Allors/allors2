//------------------------------------------------------------------------------------------------- 
// <copyright file="StepExtensions.cs" company="Allors bvba">
// Copyright 2002-2017 Allors bvba.
// 
// Dual Licensed under
//   a) the Lesser General Public Licence v3 (LGPL)
//   b) the Allors License
// 
// The LGPL License is included in the file lgpl.txt.
// The Allors License is an addendum to your contract.
// 
// Allors Platform is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// For more information visit http://www.allors.com/legal
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace Allors.Protocol.Data
{
    using Allors.Meta;

    public static class StepExtensions 
    {
        public static Allors.Data.Step Load(this Step @this, ISession session)
        {
            return new Allors.Data.Step
                       {
                           PropertyType = @this.PropertyType != null ? (IPropertyType)session.Database.ObjectFactory.MetaPopulation.Find(@this.PropertyType.Value) : null,
                           Next = @this.Next?.Load(session),
                           Include = @this.Include?.Load(session)
                       };
        }
    }
}