// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectOnPreDerive.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Domain
{
    public abstract partial class ObjectOnPreDerive
    {
        public IDerivation Derivation { get; set; }

        public ObjectOnPreDerive WithDerivation(IDerivation derivation)
        {
            this.Derivation = derivation;
            return this;
        }
    }
}
