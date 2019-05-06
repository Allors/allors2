namespace Angular.Material
{
    using System.Diagnostics.CodeAnalysis;

    using Allors.Meta;

    using Angular;

    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.PageObjects;

    public class MaterialRadioGroup : Component
    {
        public MaterialRadioGroup(IWebDriver driver, RoleType roleType, params string[] scopes)
            : base(driver)
        {
            var xpath = $"//a-mat-radiogroup{this.ByScopesPredicate(scopes)}//*[@data-allors-roletype='{roleType.IdAsNumberString}']";
            this.Selector = By.XPath(xpath);
        }

        public By Selector { get; }


        public void Select(string value)
        {
            this.Driver.WaitForAngular();
            var radioSelector = new ByChained(this.Selector, By.CssSelector($"mat-radio-button[data-allors-radio-value='{value}']"));
            var radio = this.Driver.FindElement(radioSelector);
            this.ScrollToElement(radio);
            radio.Click();
            this.Driver.WaitForAngular();
            radio.SendKeys(Keys.Space);
            this.Driver.WaitForAngular();
            radio.SendKeys(Keys.Enter);
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public class MaterialRadioGroup<T> : MaterialRadioGroup where T : Component
    {
        public MaterialRadioGroup(T page, RoleType roleType, params string[] scopes)
            : base(page.Driver, roleType, scopes)
        {
            this.Page = page;
        }

        public T Page { get; }

        public new T Select(string value)
        {
            base.Select(value);
            return this.Page;
        }
    }
}