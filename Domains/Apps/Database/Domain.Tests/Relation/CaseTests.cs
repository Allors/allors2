//------------------------------------------------------------------------------------------------- 
// <copyright file="CaseTests.cs" company="Allors bvba">
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

    
    public class CaseTests : DomainTest
    {
        [Fact]
        public void GivenCase_WhenBuild_ThenLastObjectStateEqualsCurrencObjectState()
        {
            var complaint = new CaseBuilder(this.DatabaseSession).WithDescription("Complaint").Build();

            this.DatabaseSession.Derive();
            
            Assert.Equal(new CaseStates(this.DatabaseSession).Opened, complaint.CaseState);
            Assert.Equal(complaint.LastCaseState, complaint.CaseState);
        }

        [Fact]
        public void GivenCase_WhenBuild_ThenPreviousObjectStateIsNull()
        {
            var complaint = new CaseBuilder(this.DatabaseSession).WithDescription("Complaint").Build();

            this.DatabaseSession.Derive();

            Assert.Null(complaint.PreviousCaseState);
        }

        [Fact]
        public void GivenCase_WhenConfirmed_ThenCurrentCaseStatusMustBeDerived()
        {
            var complaint = new CaseBuilder(this.DatabaseSession).WithDescription("Complaint").Build();

            this.DatabaseSession.Derive();

            Assert.Equal(1, complaint.CaseStatuses.Count);
            Assert.Equal(new CaseStates(this.DatabaseSession).Opened, complaint.CurrentCaseStatus.CaseState);

            complaint.AppsClose();

            this.DatabaseSession.Derive();

            Assert.Equal(2, complaint.CaseStatuses.Count);
            Assert.Equal(new CaseStates(this.DatabaseSession).Closed, complaint.CurrentCaseStatus.CaseState);
        }
    }
}
