using System.Linq;

namespace Components
{
    using System.Diagnostics.CodeAnalysis;
    using OpenQA.Selenium;

    public class MatTableRow : Component
    {
        public MatTableRow(IWebDriver driver, IWebElement element)
        : base(driver)
        {
            this.Element = element;
        }

        public IWebElement Element { get; }

        public MatTableCell FindCell(string name)
        {
            var cell = this.TableCell(name);
            return new MatTableCell(this.Driver, cell);
        }

        public MatTableCell[] Cells
        {
            get
            {
                var cellPath = By.CssSelector($"td");
                var cells = this.Element.FindElements(cellPath);
                return cells.Select(v => new MatTableCell(this.Driver, v)).ToArray();
            }
        }


        protected IWebElement TableCell(string name)
        {
            this.Driver.WaitForAngular();

            var cellPath = By.CssSelector($"td.mat-column-{name}");
            var cell = this.Element.FindElement(cellPath);
            return cell;
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public class MatTableRow<T> : MatTableRow where T : Component
    {
        public MatTableRow(T page, IWebElement element)
            : base(page.Driver, element)
        {
            this.Page = page;
        }

        public T Page { get; }

        public new MatTableCell<T> FindCell(string name)
        {
            var cell = this.TableCell(name);
            return new MatTableCell<T>(this.Page, cell);
        }
    }
}