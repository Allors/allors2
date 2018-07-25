namespace Intranet.Tests.Relations
{
    using System.Linq;

    using Allors;
    using Allors.Domain;

    using Intranet.Pages.Relations;

    using Xunit;

    [Collection("Test collection")]
    public class DateTest : Test
    {
        private readonly FormPage page;

        public DateTest(TestFixture fixture)
            : base(fixture)
        {
            var dashboard = this.Login();
            this.page = dashboard.Sidenav.NavigateToForm();
        }

        [Fact]
        public void Initial()
        {
            var before = new Datas(this.Session).Extent().ToArray();

            var date = this.Session.Now();
            this.page.Date.Value = date;

            this.page.Save.Click();

            this.Driver.WaitForAngular();
            this.Session.Rollback();

            var after = new Datas(this.Session).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var data = after.Except(before).First();

            Assert.True(data.ExistDate);
        }
    }
}
