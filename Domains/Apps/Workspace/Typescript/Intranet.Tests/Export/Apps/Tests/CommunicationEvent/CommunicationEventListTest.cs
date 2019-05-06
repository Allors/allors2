namespace Tests.CommunicationEventTests
{
    using src.allors.material.apps.objects.communicationevent.list;
    using Allors.Domain;
    using Allors.Meta;
    using Xunit;

    [Collection("Test collection")]
    public class CommunicationEventListTest : Test
    {
        private readonly CommunicationEventListComponent page;

        public CommunicationEventListTest(TestFixture fixture)
            : base(fixture)
        {
            var dashboard = this.Login();
            this.page = dashboard.Sidenav.NavigateToCommunicationEvents();
        }

        [Fact]
        public void Title()
        {
            Assert.Equal("Communications", this.Driver.Title);
        }

        [Fact]
        public void Table()
        {
            var communicationEvent = new CommunicationEvents(this.Session).FindBy(M.CommunicationEvent.Subject, "meeting 0");

            var cell = this.page.Table
                .FindRow(communicationEvent)
                .FindCell("subject");

            Assert.Equal("meeting 0", cell.Element.Text);
        }
    }
}
