using mars_nunit_json.Support;
using mars_nunit_json.TestDataModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using static mars_nunit_json.Support.UiTextHelper;

namespace mars.nunit.json.Pages
{
    public class EducationPage
    {
        // ===== Fields =====
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private readonly NavigationHelper _nav;
       
        // ===== Locators: Tab + Pane =====
        private readonly By EducationTabLinkBy = By.CssSelector("a[data-tab='third']");
        private readonly By EducationPaneBy = By.CssSelector("div[data-tab='third']");
        private static readonly By SuccessToastBy =By.CssSelector("div.ns-box.ns-type-success .ns-box-inner");

        // ===== Locators: Buttons =====
        private readonly By EduAddNewButtonBy = By.XPath("//div[@data-tab='third']//div[contains(@class,'ui') and contains(@class,'button') and normalize-space(.)='Add New']");
        private readonly By EduAddButtonBy = By.XPath("//div[@data-tab='third']//input[@type='button' and normalize-space(@value)='Add']");
        private readonly By UpdateButtonBy = By.XPath("//div[@data-tab='third']//input[@type='button' and normalize-space(@value)='Update']");
        private readonly By CancelButtonBy = By.CssSelector("div[data-tab='third'] input.ui.button[value='Cancel']");

        // ===== Locators: Inputs =====
        private readonly By UniversityInputBy = By.CssSelector("div[data-tab='third'] input[placeholder='College/University Name']");
        private readonly By DegreeInputBy = By.CssSelector("div[data-tab='third'] input[placeholder='Degree']");

        // ===== Locators: Selects =====
        private readonly By CountrySelectBy = By.CssSelector("div[data-tab='third'] select[name='country']");
        private readonly By TitleSelectBy = By.CssSelector("div[data-tab='third'] select[name='title']");
        private readonly By YearSelectBy = By.CssSelector("div[data-tab='third'] select[name='yearOfGraduation']");

        // ===== Locators: Grid rows + icons =====
        private static readonly By EduRowsBy = By.CssSelector("div[data-tab='third'] table tbody tr");
        private static readonly By RowDeleteIconBy = By.CssSelector("div[data-tab='third'] i.remove.icon");
        private static readonly By RowEditIconBy = By.CssSelector("i.outline.write.icon");

        // Optional explicit first-row edit icon if you prefer:
        private readonly By FirstRowEditIconBy = By.XPath("//div[@data-tab='third']//table//tbody/tr[1]//i[contains(@class,'write icon')]");

        // ===== Element accessors (all go through waits where needed) =====

        private IWebElement EducationTabLink => WaitVisible(EducationTabLinkBy);
        private IWebElement EducationPane => WaitVisible(EducationPaneBy);
        private IWebElement EduAddNewButton => WaitVisible(EduAddNewButtonBy);
        private IWebElement EduAddButton => WaitVisible(EduAddButtonBy);
        private IWebElement UpdateButton => WaitVisible(UpdateButtonBy);
        private IWebElement CancelButton => WaitVisible(CancelButtonBy);
        private IWebElement UniversityInput => WaitVisible(UniversityInputBy);
        private IWebElement DegreeInput => WaitVisible(DegreeInputBy);
        private IWebElement FirstRowEditIcon => WaitVisible(FirstRowEditIconBy);

        // ===== Select wrappers (your requested pattern) =====
        private SelectElement CountrySelect => new SelectElement(WaitVisible(CountrySelectBy));
        private SelectElement TitleSelect => new SelectElement(WaitVisible(TitleSelectBy));
        private SelectElement YearSelect => new SelectElement(WaitVisible(YearSelectBy));

        // ===== Constructor =====
        public EducationPage(IWebDriver driver,NavigationHelper nav)
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
            _driver.FindElements(EduRowsBy);

        // ===== Navigation =====

        public void OpenEducationTab()
        {
            var toast = new ToastHelper(_driver);
            try
            {
                
                EducationTabLink.Click();
            }
            catch (ElementClickInterceptedException)
            {
               
                try
                {
                    toast.CloseToast();
                    EducationTabLink.Click();
                }
                catch (WebDriverTimeoutException)
                {
                    // If no toast close button found in time, ignore and retry once
                    EducationTabLink.Click();
                }
            }
        }


        public void OpenForm()
        {
            EduAddNewButton.Click();
        }

        // ===== Form fill (no submit) =====

        public void FillEducationForm(EducationRecord record)
        {
         
            UniversityInput.Clear();
            if (!string.IsNullOrWhiteSpace(record.University))UniversityInput.SendKeys(record.University);

            if (!string.IsNullOrWhiteSpace(record.Country))
            {
                CountrySelect.SelectByText(record.Country);
              
            }
           
            if (!string.IsNullOrWhiteSpace(record.Title))
            {
                TitleSelect.SelectByText(record.Title);
            }
       
            if (!string.IsNullOrWhiteSpace(record.GraduationYear))
            {

                YearSelect.SelectByText(record.GraduationYear.ToString());
            }

           
            DegreeInput.Clear();

            if (!string.IsNullOrWhiteSpace(record.Degree))DegreeInput.SendKeys(record.Degree);
        }

        

        // ===== Submit buttons =====

        public void ClickAddButton()
        {
           
            EduAddButton.Click();
            
        }

        public void ClickUpdateButton()
        {
            UpdateButton.Click();
        }

        public void DoubleClickAddButton()
        {
            _wait.Until(ExpectedConditions.ElementToBeClickable(EduAddButton));
            var actions = new Actions(_driver);
            actions.DoubleClick(EduAddButton).Perform();
        }
        public void DoubleClickUpdateButton()
        {
            _wait.Until(ExpectedConditions.ElementToBeClickable(UpdateButton));
            var actions = new Actions(_driver);
            actions.DoubleClick(UpdateButton).Perform();
        }

        // ===== Update existing row =====

        public void EditFirstEducation(EducationRecord record)
        {
            // Click edit icon for first row
            FirstRowEditIcon.Click();

            UniversityInput.Clear();
            UniversityInput.SendKeys(record.University);

            CountrySelect.SelectByText(record.Country);
            TitleSelect.SelectByText(record.Title);
            YearSelect.SelectByText(record.GraduationYear.ToString());

            DegreeInput.Clear();
            DegreeInput.SendKeys(record.Degree);
        }

        // ===== Delete specific row =====
        
        public void DeleteEducation(EducationRecord record)
        {
            var rows = _driver.FindElements(EduRowsBy);

            foreach (var row in rows)
            {
                var cells = row.FindElements(By.TagName("td"));
                if (cells.Count < 5) continue;

                var countryText = cells[0].Text.Trim();
                var universityText = cells[1].Text.Trim();
                var titleText = cells[2].Text.Trim();
                var degreeText = cells[3].Text.Trim();
                var yearText = cells[4].Text.Trim();

                if (EqNorm(countryText, record.Country)
                    && EqNorm(universityText, record.University)
                    && EqNorm(titleText, record.Title)
                    && EqNorm(degreeText, record.Degree)
                    && EqNorm(yearText, record.GraduationYear.ToString()))
                {
                    var deleteIcon = row.FindElement(RowDeleteIconBy);
                    deleteIcon.Click();
                    _wait.Until(ExpectedConditions.ElementIsVisible(SuccessToastBy));
                    break;
                }
            }
        }

        // ===== Delete all rows =====

        public void DeleteAllEducation()
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


                    _wait.Until(d => d.FindElements(EduRowsBy).Count < before);
                }
            }
        }


        // ===== Cancel =====

        public void CancelEducationAdd()
        {
            CancelButton.Click();
        }



        // ===== Grid read helpers =====

        public List<EducationRecord> GetEducationRows()
        {
            List<EducationRecord> list = new();
            var rows = GetRows();

            foreach (var row in rows)
            {
                var cells = row.FindElements(By.TagName("td"));
                EducationRecord item = new();

                if (cells.Count > 0)
                    item.Country = cells[0].Text.Trim();
                if (cells.Count > 1)
                    item.University = cells[1].Text.Trim();
                if (cells.Count > 2)
                    item.Title = cells[2].Text.Trim();
                if (cells.Count > 3)
                    item.Degree = cells[3].Text.Trim();
                if (cells.Count > 4)
                    item.GraduationYear = cells[4].Text.Trim();

                list.Add(item);
            }

            return list;
        }

        public SelectElement GetSelectElement(string dropdown)
        {
            return dropdown.ToLower() switch
            {
                "country" => CountrySelect,
                "title" => TitleSelect,
                "year" => YearSelect,
                _ => throw new ArgumentException($"Unknown dropdown: {dropdown}")
            };
        }

        public List<string> GetRowTexts()
        {
            var rows = GetRows();
            return rows
                .Select(r => r.Text.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();
        }

       

        public int CountEducationRows(EducationRecord record)
        {
            int count = 0;
            var rows = GetRows();

            foreach (var row in rows)
            {
                var text = row.Text.Trim();

                if (text.Contains(record.University, StringComparison.OrdinalIgnoreCase)
                    && text.Contains(record.Country, StringComparison.OrdinalIgnoreCase)
                    && text.Contains(record.Title, StringComparison.OrdinalIgnoreCase)
                    && text.Contains(record.Degree, StringComparison.OrdinalIgnoreCase)
                    && text.Contains(record.GraduationYear.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return count;
        }
      
      
        public string GetEducationDetails()
        {
            var rows = GetEducationRows();
            return string.Join(", ",
                rows.ConvertAll(r =>
                    $"{r.University}/{r.Country}/{r.Title}/{r.Degree}/{r.GraduationYear}"));
        }

        public int GetRowCount() => GetRows().Count;
    }
}
