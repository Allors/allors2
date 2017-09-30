//------------------------------------------------------------------------------------------------- 
// <copyright file="FaxCommunicationTests.cs" company="Allors bvba">
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

    
    public class FaxCommunicationTests : DomainTest
    {
        [Fact]
        public void GivenFaxCommunicationIsBuild_WhenDeriving_ThenStatusIsSet()
        {
            var communication = new FaxCommunicationBuilder(this.DatabaseSession)
                .WithSubject("subject")
                .WithOwner(new PersonBuilder(this.DatabaseSession).WithLastName("owner").WithPersonRole(new PersonRoles(this.DatabaseSession).Employee).Build())
                .WithOriginator(new PersonBuilder(this.DatabaseSession).WithLastName("originator").WithPersonRole(new PersonRoles(this.DatabaseSession).Customer).Build())
                .WithReceiver(new PersonBuilder(this.DatabaseSession).WithLastName("receiver").WithPersonRole(new PersonRoles(this.DatabaseSession).Customer).Build())
                .Build();

            Assert.False(this.DatabaseSession.Derive(false).HasErrors);

            Assert.Equal(communication.CommunicationEventState, new CommunicationEventStates(this.DatabaseSession).Scheduled);
            Assert.Equal(communication.CommunicationEventState, communication.LastCommunicationState);
        }

        [Fact]
        public void GivenFaxCommunication_WhenDeriving_ThenInvolvedPartiesAreDerived()
        {
            var owner = new PersonBuilder(this.DatabaseSession).WithLastName("owner").WithPersonRole(new PersonRoles(this.DatabaseSession).Employee).Build();
            var originator = new PersonBuilder(this.DatabaseSession).WithLastName("originator").WithPersonRole(new PersonRoles(this.DatabaseSession).Customer).Build();
            var receiver = new PersonBuilder(this.DatabaseSession).WithLastName("receiver").WithPersonRole(new PersonRoles(this.DatabaseSession).Customer).Build();

            this.DatabaseSession.Derive();
            this.DatabaseSession.Commit();

            var communication = new FaxCommunicationBuilder(this.DatabaseSession)
                .WithSubject("subject")
                .WithOwner(owner)
                .WithOriginator(originator)
                .WithReceiver(receiver)
                .Build();

            this.DatabaseSession.Derive();

            Assert.Equal(3, communication.InvolvedParties.Count);
            Assert.Contains(owner, communication.InvolvedParties);
            Assert.Contains(originator, communication.InvolvedParties);
            Assert.Contains(receiver, communication.InvolvedParties);
        }

        [Fact]
        public void GivenFaxCommunication_WhenOriginatorIsDeleted_ThenCommunicationEventIsDeleted()
        {
            var owner = new PersonBuilder(this.DatabaseSession).WithLastName("owner").WithPersonRole(new PersonRoles(this.DatabaseSession).Employee).Build();
            var originator = new PersonBuilder(this.DatabaseSession).WithLastName("originator").WithPersonRole(new PersonRoles(this.DatabaseSession).Customer).Build();
            var receiver = new PersonBuilder(this.DatabaseSession).WithLastName("receiver").WithPersonRole(new PersonRoles(this.DatabaseSession).Customer).Build();

            this.DatabaseSession.Derive();
            this.DatabaseSession.Commit();

            new FaxCommunicationBuilder(this.DatabaseSession)
                .WithSubject("Hello world!")
                .WithOwner(owner)
                .WithOriginator(originator)
                .WithReceiver(receiver)
                .Build();

            this.DatabaseSession.Derive();

            Assert.Equal(1, this.DatabaseSession.Extent<FaxCommunication>().Count);

            originator.Delete();
            this.DatabaseSession.Derive();

            Assert.Equal(0, this.DatabaseSession.Extent<FaxCommunication>().Count);
        }
    }
}