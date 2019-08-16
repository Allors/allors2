// <copyright file="AutoCompleteOptionsTest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using src.allors.material.custom.tests.form;

namespace Tests
{
    using System.Linq;

    using Allors.Domain;
    using Allors.Meta;

    using Components;


    using Xunit;

    [Collection("Test collection")]
    public class AutoCompleteOptionsTest : Test
    {
        private readonly FormComponent page;

        public AutoCompleteOptionsTest(TestFixture fixture)
            : base(fixture)
        {
            this.Login();
            this.page = this.Sidenav.NavigateToForm();
        }

        [Fact]
        public void Initial()
        {
            var jane = new People(this.Session).FindBy(M.Person.UserName, "jane@doe.org");

            var before = new Datas(this.Session).Extent().ToArray();

            this.page.AutocompleteOptions.Select("jane", "jane@doe.org");

            this.page.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Session.Rollback();

            var after = new Datas(this.Session).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var data = after.Except(before).First();

            Assert.Equal(jane, data.AutocompleteOptions);
        }
    }
}
