// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICacheService.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Services
{
    public interface ICacheService : IStateful
    {
        TValue Get<TKey, TValue>();

        void Set<TKey, TValue>(TValue value);

        void Clear<TKey>();
    }
}
