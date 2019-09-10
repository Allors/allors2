// <copyright file="IClassCache.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Adapters
{
    using Allors.Meta;

    public interface IClassCache
    {
        bool TryGet(long objectId, out IClass @class);

        void Set(long objectId, IClass @class);

        void Invalidate();

        void Invalidate(long[] objectsToInvalidate);
    }
}