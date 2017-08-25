// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NonSerialisedInventoryItemObjectStates.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Allors.Domain
{
    using System;

    public partial class NonSerialisedInventoryItemObjectStates
    {
        private static readonly Guid GoodId = new Guid("6806CC54-3AA7-4510-A209-99F92D5C1D58");
        private static readonly Guid BeingReparedId = new Guid("ABD19809-9E27-4ee6-BEA1-637902260F57");
        private static readonly Guid SlightlyDamagedId = new Guid("EE4C6034-7320-4209-A0AA-6067B20AC418");
        private static readonly Guid DefectiveId = new Guid("C0E10011-1BA4-412f-B426-103C1C11B879");
        private static readonly Guid ScrapId = new Guid("CF51C221-111C-4666-8E97-CC060643C5FD");

        private UniquelyIdentifiableCache<NonSerialisedInventoryItemObjectState> stateCache;

        public NonSerialisedInventoryItemObjectState Good => this.StateCache.Get(GoodId);

        public NonSerialisedInventoryItemObjectState BeingRepared => this.StateCache.Get(BeingReparedId);

        public NonSerialisedInventoryItemObjectState SlightlyDamaged => this.StateCache.Get(SlightlyDamagedId);

        public NonSerialisedInventoryItemObjectState Defective => this.StateCache.Get(DefectiveId);

        public NonSerialisedInventoryItemObjectState Scrap => this.StateCache.Get(ScrapId);

        private UniquelyIdentifiableCache<NonSerialisedInventoryItemObjectState> StateCache => this.stateCache ?? (this.stateCache = new UniquelyIdentifiableCache<NonSerialisedInventoryItemObjectState>(this.Session));

        protected override void AppsSetup(Setup setup)
        {
            base.AppsSetup(setup);

            var englishLocale = new Locales(this.Session).EnglishGreatBritain;
            var dutchLocale = new Locales(this.Session).DutchNetherlands;

            new NonSerialisedInventoryItemObjectStateBuilder(this.Session)
                .WithUniqueId(GoodId)
                .WithName("Good")
                .Build();

            new NonSerialisedInventoryItemObjectStateBuilder(this.Session)
                .WithUniqueId(BeingReparedId)
                .WithName("Being Repared")
                .Build();

            new NonSerialisedInventoryItemObjectStateBuilder(this.Session)
                .WithUniqueId(SlightlyDamagedId)
                .WithName("Slightly Damaged")
                .Build();

            new NonSerialisedInventoryItemObjectStateBuilder(this.Session)
                .WithUniqueId(DefectiveId)
                .WithName("Defective")
                .Build();

            new NonSerialisedInventoryItemObjectStateBuilder(this.Session)
                .WithUniqueId(ScrapId)
                .WithName("Scrap")
                .Build();
        }
    }
}