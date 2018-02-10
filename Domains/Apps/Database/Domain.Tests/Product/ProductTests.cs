//------------------------------------------------------------------------------------------------- 
// <copyright file="ProductTests.cs" company="Allors bvba">
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

    
    public class ProductTests : DomainTest
    {
        [Fact]
        public void GivenDeliverableCoredService_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            this.Session.Derive();
            this.Session.Commit();

            var builder = new DeliverableBasedServiceBuilder(this.Session);
            var deliverableBasedService = builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithVatRate(vatRate21);
            deliverableBasedService = builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithName("service");
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenDeliverableCoredServiceWithPrimaryProductCategoryWithoutProductCategory_WhenDeriving_ThenFirstProductCategoryIsCopiedFromPrimaryCategory()
        {
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var productCategory = new ProductCategoryBuilder(this.Session)
                .WithName("category")
                .Build();

            var deliverableBasedService = new DeliverableBasedServiceBuilder(this.Session)
                .WithName("service")
                .WithPrimaryProductCategory(productCategory)
                .WithVatRate(vatRate21)
                .Build();

            this.Session.Derive(); 
            
            Assert.Contains(productCategory, deliverableBasedService.ProductCategories);
        }

        [Fact]
        public void GivenDeliverableCoredServiceWithoutPrimaryProductCategoryWithOneProductCategory_WhenDeriving_ThenPrimaryProductCategoryIsCopiedFromCategory()
        {
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var productCategory = new ProductCategoryBuilder(this.Session)
                .WithName("category")
                .Build();

            var deliverableBasedService = new DeliverableBasedServiceBuilder(this.Session)
                .WithName("service")
                .WithProductCategory(productCategory)
                .WithVatRate(vatRate21)
                .Build();

            this.Session.Derive(); 

            Assert.Equal(productCategory, deliverableBasedService.PrimaryProductCategory);
        }

        [Fact]
        public void GivenGood_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var finishedGood = new FinishedGoodBuilder(this.Session)
                .WithInternalOrganisation(this.InternalOrganisation)
                .WithName("finishedGood")
                .WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised)
                .Build();
            
                this.Session.Derive();
            this.Session.Commit();
            
            var builder = new GoodBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();
            
            builder.WithName("good");
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithVatRate(vatRate21);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithFinishedGood(finishedGood);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            builder.WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised);
            var good = builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            good.RemoveFinishedGood();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenGoodWithPrimaryProductCategoryWithoutProductCategory_WhenDeriving_ThenFirstProductCategoryIsCopiedFromPrimaryCategory()
        {
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var productCategory = new ProductCategoryBuilder(this.Session)
                .WithName("category")
                .Build();

            var good = new GoodBuilder(this.Session)
                .WithName("good")
                .WithSku("10101")
                .WithVatRate(vatRate21)
                .WithPrimaryProductCategory(productCategory)
                .WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .Build();

            this.Session.Derive(); 

            Assert.Contains(productCategory, good.ProductCategories);
        }

        [Fact]
        public void GivenGoodWithoutPrimaryProductCategoryWithOneProductCategory_WhenDeriving_ThenPrimaryProductCategoryIsCopiedFromCategory()
        {
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var productCategory = new ProductCategoryBuilder(this.Session)
                .WithName("category")
                .Build();

            var good = new GoodBuilder(this.Session)
                .WithName("good")
                .WithSku("10101")
                .WithVatRate(vatRate21)
                .WithProductCategory(productCategory)
                .WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .Build();

            this.Session.Derive(); 

            Assert.Equal(productCategory, good.PrimaryProductCategory);
        }

        [Fact]
        public void GivenGoodWithProductCategory_WhenDeriving_ThenProductCategoriesExpandedIsSet()
        {
            var productCategory1 = new ProductCategoryBuilder(this.Session)
                .WithName("1")
                .Build();
            var productCategory2 = new ProductCategoryBuilder(this.Session)
                .WithName("2")
                .Build();
            var productCategory11 = new ProductCategoryBuilder(this.Session)
                .WithName("1.1")
                .WithParent(productCategory1)
                .WithParent(productCategory2)
                .Build();
            var productCategory12 = new ProductCategoryBuilder(this.Session)
                .WithName("1.2")
                .WithParent(productCategory1)
                .WithParent(productCategory2)
                .Build();
            var productCategory111 = new ProductCategoryBuilder(this.Session)
                .WithName("1.1.1")
                .WithParent(productCategory11)
                .Build();
            var productCategory121 = new ProductCategoryBuilder(this.Session)
                .WithName("1.2.1")
                .WithParent(productCategory12)
                .Build();

            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var good = new GoodBuilder(this.Session)
                .WithName("good")
                .WithSku("10101")
                .WithPrimaryProductCategory(productCategory111)
                .WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .WithVatRate(vatRate21)
                .Build();

            this.Session.Derive(); 

            Assert.Equal(4, good.ProductCategoriesExpanded.Count);
            Assert.Contains(productCategory111, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory11, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory1, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory2, good.ProductCategoriesExpanded);

            good.AddProductCategory(productCategory121);

            this.Session.Derive();

            Assert.Equal(6, good.ProductCategoriesExpanded.Count);
            Assert.Contains(productCategory111, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory121, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory11, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory12, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory1, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory2, good.ProductCategoriesExpanded);
        }

        [Fact]
        public void GivenGood_WhenProductCategoryParentsAreInserted_ThenProductCategoriesExpandedAreRecalculated()
        {
            var productCategory1 = new ProductCategoryBuilder(this.Session)
                .WithName("1")
                .Build();
            var productCategory2 = new ProductCategoryBuilder(this.Session)
                .WithName("2")
                .Build();
            var productCategory11 = new ProductCategoryBuilder(this.Session)
                .WithName("1.1")
                .WithParent(productCategory1)
                .WithParent(productCategory2)
                .Build();
            var productCategory12 = new ProductCategoryBuilder(this.Session)
                .WithName("1.2")
                .WithParent(productCategory1)
                .WithParent(productCategory2)
                .Build();
            var productCategory111 = new ProductCategoryBuilder(this.Session)
                .WithName("1.1.1")
                .WithParent(productCategory11)
                .Build();
            var productCategory121 = new ProductCategoryBuilder(this.Session)
                .WithName("1.2.1")
                .WithParent(productCategory12)
                .Build();

            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var good = new GoodBuilder(this.Session)
                .WithName("good")
                .WithSku("10101")
                .WithPrimaryProductCategory(productCategory111)
                .WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .WithVatRate(vatRate21)
                .Build();

            this.Session.Derive(); 

            Assert.Equal(4, good.ProductCategoriesExpanded.Count);
            Assert.Contains(productCategory111, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory11, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory1, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory2, good.ProductCategoriesExpanded);

            var productCategory3 = new ProductCategoryBuilder(this.Session)
                .WithName("3")
                .Build();
            productCategory11.AddParent(productCategory3);

            this.Session.Derive();

            Assert.Equal(5, good.ProductCategoriesExpanded.Count);
            Assert.Contains(productCategory111, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory11, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory1, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory2, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory3, good.ProductCategoriesExpanded);

            good.AddProductCategory(productCategory121);

            this.Session.Derive();

            var productCategory13 = new ProductCategoryBuilder(this.Session)
                .WithName("1.3")
                .WithParent(productCategory1)
                .Build();
            productCategory121.AddParent(productCategory13);

            this.Session.Derive();

            Assert.Equal(8, good.ProductCategoriesExpanded.Count);
            Assert.Contains(productCategory111, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory121, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory11, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory12, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory13, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory1, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory2, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory3, good.ProductCategoriesExpanded);
        }

        [Fact]
        public void GivenGood_WhenProductCategoryParentsAreRemoved_ThenProductCategoriesExpandedAreRecalculated()
        {
            var productCategory1 = new ProductCategoryBuilder(this.Session)
                .WithName("1")
                .Build();
            var productCategory2 = new ProductCategoryBuilder(this.Session)
                .WithName("2")
                .Build();
            var productCategory11 = new ProductCategoryBuilder(this.Session)
                .WithName("1.1")
                .WithParent(productCategory1)
                .WithParent(productCategory2)
                .Build();
            var productCategory12 = new ProductCategoryBuilder(this.Session)
                .WithName("1.2")
                .WithParent(productCategory1)
                .WithParent(productCategory2)
                .Build();
            var productCategory111 = new ProductCategoryBuilder(this.Session)
                .WithName("1.1.1")
                .WithParent(productCategory11)
                .Build();

            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var good = new GoodBuilder(this.Session)
                .WithName("good")
                .WithSku("10101")
                .WithPrimaryProductCategory(productCategory111)
                .WithInventoryItemKind(new InventoryItemKinds(this.Session).NonSerialised)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Piece)
                .WithVatRate(vatRate21)
                .Build();

            this.Session.Derive(); 

            Assert.Equal(4, good.ProductCategoriesExpanded.Count);
            Assert.Contains(productCategory111, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory11, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory1, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory2, good.ProductCategoriesExpanded);

            productCategory11.RemoveParent(productCategory2);

            this.Session.Derive();

            Assert.Equal(3, good.ProductCategoriesExpanded.Count);
            Assert.Contains(productCategory111, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory11, good.ProductCategoriesExpanded);
            Assert.Contains(productCategory1, good.ProductCategoriesExpanded);
        }

        [Fact]
        public void GivenTimeAndMaterialsService_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();

            this.Session.Derive();
            this.Session.Commit();

            var builder = new TimeAndMaterialsServiceBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithName("good");
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithVatRate(vatRate21);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenTimeAndMaterialsServiceWithPrimaryProductCategoryWithoutProductCategory_WhenDeriving_ThenFirstProductCategoryIsCopiedFromPrimaryCategory()
        {
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var productCategory = new ProductCategoryBuilder(this.Session)
                .WithName("category")
                .Build();

            var timeAndMaterialsService = new TimeAndMaterialsServiceBuilder(this.Session)
                .WithName("TimeAndMaterialsService")
                .WithPrimaryProductCategory(productCategory)
                .WithVatRate(vatRate21)
                .Build();

            this.Session.Derive(); 

            Assert.Contains(productCategory, timeAndMaterialsService.ProductCategories);
        }

        [Fact]
        public void GivenTimeAndMaterialsServiceWithoutPrimaryProductCategoryWithOneProductCategory_WhenDeriving_ThenPrimaryProductCategoryIsCopiedFromCategory()
        {
            var vatRate21 = new VatRateBuilder(this.Session).WithRate(21).Build();
            var productCategory = new ProductCategoryBuilder(this.Session)
                .WithName("category")
                .Build();

            var timeAndMaterialsService = new TimeAndMaterialsServiceBuilder(this.Session)
                .WithName("TimeAndMaterialsService")
                .WithProductCategory(productCategory)
                .WithVatRate(vatRate21)
                .Build();

            this.Session.Derive(); 

            Assert.Equal(productCategory, timeAndMaterialsService.PrimaryProductCategory);
        }
    }
}
