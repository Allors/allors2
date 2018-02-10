//------------------------------------------------------------------------------------------------- 
// <copyright file="InventoryItemTests.cs" company="Allors bvba">
// Copyright 2002-2009 Allors bvba.
// 
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// 
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// 
// Allors Platform is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// For more information visit http://www.allors.com/legal
// </copyright>
// <summary>Defines the MediaTests type.</summary>
//-------------------------------------------------------------------------------------------------

namespace Allors.Domain
{
    using Xunit;

    
    public class InventoryItemTests : DomainTest
    {
        [Fact]
        public void GivenInventoryItem_WhenDeriving_ThenProductCategoriesAreDerived()
        {
            var level1 = new ProductCategoryBuilder(this.Session)
                .WithName("level1")
                .Build();
            var level2 = new ProductCategoryBuilder(this.Session)
                .WithName("level2")
                .WithParent(level1)
                .Build();
            var level3 = new ProductCategoryBuilder(this.Session)
                .WithName("level3")
                .WithParent(level2)
                .Build();
            var category = new ProductCategoryBuilder(this.Session)
                .WithName("category")
                .Build();

            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var good = new GoodBuilder(this.Session)
                .WithName("good")
                .WithSku("10101")
                .WithVatRate(vatRate21)
                .WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .WithProductCategory(level3)
                .WithProductCategory(category)
                .Build();

            var goodInventory = new NonSerialisedInventoryItemBuilder(this.Session)
                .WithGood(good)
                .Build();

            this.Session.Derive();

            Assert.Equal(4, goodInventory.DerivedProductCategories.Count);
            Assert.Contains(level3, goodInventory.DerivedProductCategories);
            Assert.Contains(level2, goodInventory.DerivedProductCategories);
            Assert.Contains(level1, goodInventory.DerivedProductCategories);
            Assert.Contains(category, goodInventory.DerivedProductCategories);
        }
    }
}
