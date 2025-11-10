using mars_nunit_json.Support;
using mars_nunit_json.TestDataModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using static mars_nunit_json.Support.UiTextHelper;

namespace mars.nunit.json.Pages
{
    public class CertificationPage
    {
        // ===== Fields =====
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private readonly NavigationHelper _nav;


        // ===== Locators: Tab + Pane =====
        private readonly By CertificationTabLinkBy = By.CssSelector("a[data-tab='fourth']");
        private readonly By CertificationPaneBy = By.CssSelector("div[data-tab='fourth']");
        private static readonly By SuccessToastBy =
       By.CssSelector("div.ns-box.ns-type-success .ns-box-inner");

        // ===== Locators: Buttons =====
        private readonly By CertAddNewButtonBy =
            By.XPath("//div[@data-tab='fourth']//div[contains(@class,'ui') and contains(@class,'button') and normalize-space(.)='Add New']");

        private readonly By CertAddButtonBy =
            By.XPath("//div[@data-tab='fourth']//input[@type='button' and normalize-space(@value)='Add']");

        private readonly By UpdateButtonBy =
            By.XPath("//div[@data-tab='fourth']//input[@type='button' and normalize-space(@value)='Update']");

        private readonly By CancelButtonBy =
            By.CssSelector("div[data-tab='fourth'] input.ui.button[value='Cancel']");

        // ===== Locators: Inputs =====
        private readonly By CertificateInputBy =
            By.CssSelector("div[data-tab='fourth'] input[placeholder='Certificate or Award']");

        private readonly By FromInputBy =
            By.CssSelector("input.received-from.capitalize[name='certificationFrom']");

        // ===== Locators: Selects =====
        private readonly By YearSelectBy =
            By.CssSelector("div[data-tab='fourth'] select[name='certificationYear']");

        // ===== Locators: Grid rows + icons =====
        private static readonly By CertRowsBy =
            By.CssSelector("div[data-tab='fourth'] table tbody tr");

        private static readonly By RowDeleteIconBy =
            By.CssSelector("div[data-tab='fourth'] i.remove.icon");

        private static readonly By RowEditIconBy =
            By.CssSelector("div[data-tab='fourth'] i.outline.write.icon");

        private readonly By FirstRowEditIconBy =
            By.XPath("//div[@data-tab='fourth']//table//tbody/tr[1]//i[contains(@class,'write icon')]");

        // ===== Element accessors =====
        private IWebElement CertificationTabLink => WaitVisible(CertificationTabLinkBy);
        private IWebElement CertificationPane => WaitVisible(CertificationPaneBy);
        private IWebElement CertAddNewButton => WaitVisible(CertAddNewButtonBy);
        private IWebElement CertAddButton => WaitVisible(CertAddButtonBy);
        private IWebElement UpdateButton => WaitVisible(UpdateButtonBy);
        private IWebElement CancelButton => WaitVisible(CancelButtonBy);
        private IWebElement CertificateInput => WaitVisible(CertificateInputBy);
        private IWebElement FromInput => WaitVisible(FromInputBy);
        private IWebElement FirstRowEditIcon => WaitVisible(FirstRowEditIconBy);

        private SelectElement YearSelect => new SelectElement(WaitVisible(YearSelectBy));

        // ===== Constructor =====
        public CertificationPage(IWebDriver driver, NavigationHelper nav)
        {
            _driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            _wait.IgnoreExceptionTypes(
                typeof(NoSuchElementException),
                typeof(StaleElementReferenceException),
                typeof(UnhandledAlertException));

            _nav = nav;
        }

        // ===== Core wait helpers =====

        private IWebElement WaitVisible(By locator) =>
            _wait.Until(ExpectedConditions.ElementIsVisible(locator));

        private IReadOnlyCollection<IWebElement> GetRows() =>
            _driver.FindElements(CertRowsBy);

        // ===== Navigation =====

        public void OpenCertificationTab()
        {

            var toast = new ToastHelper(_driver);
            try
            {
                CertificationTabLink.Click();
            }
            catch (ElementClickInterceptedException)
            {
                try
                {
                    toast.CloseToast();
                    CertificationTabLink.Click();
                }
                catch (WebDriverTimeoutException)
                {
                    CertificationTabLink.Click();
                }
            }
        }

        public void OpenForm()
        {
            CertAddNewButton.Click();
        }

        // ===== Form fill (no submit) =====

        public void FillCertificationForm(CertificationRecord record)
        {
            CertificateInput.Clear();
            if (!string.IsNullOrWhiteSpace(record.Certificate))
                CertificateInput.SendKeys(record.Certificate);

            FromInput.Clear();
            if (!string.IsNullOrWhiteSpace(record.From))
                FromInput.SendKeys(record.From);

            if (!string.IsNullOrWhiteSpace(record.Year))
                YearSelect.SelectByText(record.Year);
        }

        // ===== Submit buttons =====

        public void ClickAddButton()
        {
            var toast = new ToastHelper(_driver);
            CertAddButton.Click();
         

        }

        public void ClickUpdateButton()
        {
            UpdateButton.Click();
        }

        public void DoubleClickAddButton()
        {
            _wait.Until(ExpectedConditions.ElementToBeClickable(CertAddButton));
            var actions = new Actions(_driver);
            actions.DoubleClick(CertAddButton).Perform();
        }

        public void DoubleClickUpdateButton()
        {
            _wait.Until(ExpectedConditions.ElementToBeClickable(UpdateButton));
            var actions = new Actions(_driver);
            actions.DoubleClick(UpdateButton).Perform();
        }

        // ===== Cancel =====

        public void CancelCertificationAdd()
        {
            CancelButton.Click();
        }

        // ===== Edit existing row =====

        public void EditFirstCertification(CertificationRecord record)
        {
            FirstRowEditIcon.Click();

            CertificateInput.Clear();
            CertificateInput.SendKeys(record.Certificate);

            FromInput.Clear();
            FromInput.SendKeys(record.From);

            if (!string.IsNullOrWhiteSpace(record.Year))
                YearSelect.SelectByText(record.Year);
        }

        // ===== Delete specific row =====

        public void DeleteCertification(CertificationRecord record)
        {
            var rows = _driver.FindElements(CertRowsBy);

            foreach (var row in rows)
            {
                var cells = row.FindElements(By.TagName("td"));
                if (cells.Count < 3) continue;

                var certificateText = cells[0].Text.Trim();
                var fromText = cells[1].Text.Trim();
                var yearText = cells[2].Text.Trim();

                if (EqNorm(certificateText, record.Certificate)
                    && EqNorm(fromText, record.From)
                    && EqNorm(yearText, record.Year.ToString()))
                {
                    var deleteIcon = row.FindElement(RowDeleteIconBy);
                    deleteIcon.Click();
                    _wait.Until(ExpectedConditions.ElementIsVisible(SuccessToastBy));
                    break;
                }
            }
        }

        // ===== Delete all rows =====

        public void DeleteAllCertification()
        {
            while (true)
            {
                var rows = GetRows();
                if (rows.Count == 0)
                    break;

                var before = rows.Count;

                try
                {
                    var firstRow = rows.First();
                    var deleteIcon = firstRow.FindElement(RowDeleteIconBy);
                    deleteIcon.Click();
                }
                catch (StaleElementReferenceException)
                {
                    
                }

                _wait.Until(d => d.FindElements(CertRowsBy).Count < before);
            }
        }

       

        // ===== Grid read helpers =====

        public List<CertificationRecord> GetCertificationRows()
        {
            List<CertificationRecord> list = new();
            var rows = GetRows();

            foreach (var row in rows)
            {
                var cells = row.FindElements(By.TagName("td"));
                CertificationRecord item = new();

                if (cells.Count > 0)
                    item.Certificate = cells[0].Text.Trim();
                if (cells.Count > 1)
                    item.From = cells[1].Text.Trim();
                if (cells.Count > 2)
                    item.Year = cells[2].Text.Trim();

                list.Add(item);
            }

            return list;
        }

        public int CountCertificationRows(CertificationRecord record)
        {
            int count = 0;
            var rows = GetRows();

            foreach (var row in rows)
            {
                var text = row.Text.Trim();

                if (text.Contains(record.Certificate ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                    && text.Contains(record.From ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                    && text.Contains(record.Year ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return count;
        }
      
        public string GetCertificationDetails()
        {
            var rows = GetCertificationRows();
            return string.Join(", ",
                rows.ConvertAll(r =>
                    $"{r.Certificate}/{r.From}/{r.Year}"));
        }


        public int GetRowCount() => GetRows().Count;
    }
}
