// FILE: LoginPage.cs
// ROLE: Authentication POM — opens Sign In, performs login, surfaces validation/lockout signals.

using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using mars_nunit_json.Support;
using SeleniumExtras.WaitHelpers;

namespace mars_nunit_json.Pages
{
    public class LoginPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private readonly NavigationHelper _nav;
        public IWebDriver Driver => _driver;

        // Locators
        private readonly By SignInLink = By.XPath("//a[@class='item' and text()='Sign In']");
        private readonly By UsernameField = By.CssSelector("input[name='email'][placeholder='Email address']");
        private readonly By PasswordField = By.CssSelector("input[type='password']");
        private readonly By LoginButton = By.XPath("//button[normalize-space()='Login']");
        private readonly By SuccessButton = By.XPath("//button[normalize-space()='Sign Out' ] | //a[normalize-space()='Sign Out' ]");
        private readonly By PasswordErrorPrompt = By.XPath("//div[text()='Password must be at least 6 characters']");
        private readonly By EmailErrorPrompt = By.XPath("//div[text()='Please enter a valid email address']");
        private readonly By ErrorToast = By.XPath("//div[@class='ns-box-inner' and text()='Confirm your email']");
        private readonly By LockoutMessage = By.XPath("//div[contains(text(),'too many attempts') or contains(text(),'locked')]");

        // Creates page with driver, wait, and navigation
        public LoginPage(IWebDriver driver, NavigationHelper nav)
        {
            _driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(3));
            _nav = nav;
        }

        // Opens the Sign In modal
        public void OpenSignIn()
        {
            _nav.NavigateTo("/");
            _driver.Manage().Window.Maximize();
            _wait.Until(ExpectedConditions.ElementToBeClickable(SignInLink)).Click();
        }

        // Enters credentials and clicks Login
        public void Login(string username, string password)
        {
            var userEl = _wait.Until(ExpectedConditions.ElementIsVisible(UsernameField));
            userEl.Clear();
            userEl.SendKeys(username);

            var passEl = _wait.Until(ExpectedConditions.ElementIsVisible(PasswordField));
            passEl.Clear();
            passEl.SendKeys(password);

            _wait.Until(ExpectedConditions.ElementToBeClickable(LoginButton)).Click();
        }

        // Waits until a logged-in signal is visible
        public void WaitUntilLoggedIn(int seconds = 5)
        {
            new WebDriverWait(_driver, TimeSpan.FromSeconds(seconds))
                .Until(_ => IsLoggedIn());
        }

        // Gets success indicator text (e.g., Sign Out)
        public string GetSuccessMessage()
        {
            return _wait.Until(d => d.FindElement(SuccessButton)).Text;
        }

        // Returns email validation error text or ""
        public string GetEmailError()
        {
            try
            {
                var list = _driver.FindElements(EmailErrorPrompt);
                if (list.Count > 0 && list[0].Displayed)
                    return list[0].Text.Trim();
            }
            catch { }
            return string.Empty;
        }

        // Returns password validation error text or ""
        public string GetPasswordError()
        {
            try
            {
                var list = _driver.FindElements(PasswordErrorPrompt);
                if (list.Count > 0 && list[0].Displayed)
                    return list[0].Text.Trim();
            }
            catch { }
            return string.Empty;
        }

        // Returns popup/toast error text or ""
        public string GetPopupError()
        {
            try
            {
                var list = _driver.FindElements(ErrorToast);
                if (list.Count > 0 && list[0].Displayed)
                    return list[0].Text.Trim();
            }
            catch { }
            return string.Empty;
        }

        // True if logged-in indicator is present
        public bool IsLoggedIn()
        {
            try
            {
                return _driver.FindElements(SuccessButton).Count > 0;
            }
            catch
            {
                return false;
            }
        }

        // Tries to read success indicator text
        public string TryGetSuccess()
        {
            try
            {
                var els = _driver.FindElements(SuccessButton);
                return els.Count > 0 ? els[0].Text : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        // True if lockout message is visible
        public bool IsLockoutMessageVisible()
        {
            try
            {
                var elements = _driver.FindElements(LockoutMessage);
                return elements.Count > 0 && elements[0].Displayed;
            }
            catch
            {
                return false;
            }
        }
    }
}
