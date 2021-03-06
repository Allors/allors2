// <copyright file="FeeTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Domain
{
    using Xunit;

    public class FeeTests : DomainTest
    {
        [Fact]
        public void GivenFee_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new FeeBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithAmount(1);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            builder.WithPercentage(1);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);
        }
    }
}
