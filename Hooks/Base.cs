using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using mars.nunit.json.Pages;
using mars_nunit_json.Config;
using mars_nunit_json.Pages;
using mars_nunit_json.Support;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System;
using System.IO;
using System.Net; 

namespace mars_nunit_json.Hooks
{
    // Base test fixture:
    // - Creates WebDriver
    // - Logs in once per test
    // - Manages ExtentReports + screenshots
    [TestFixture]
    public class Base : LoggerHelper
    {
        // Shared test infrastructure
        protected IWebDriver Driver;
        protected static ExtentReports Extent;
        protected static ExtentTest Test;
        protected TestSettings Settings;

        // Page objects and helpers
        protected NavigationHelper Nav;
        protected LoginPage LoginPage;
        protected EducationPage EducationPage;
        protected static CertificationPage CertificationPage;
        protected static ToastHelper Toasts;
        private static string HtmlSafe(string s)
        {
            return WebUtility.HtmlEncode(s ?? string.Empty);
        }

        // =======================
        // GLOBAL ONE-TIME SETUP
        // =======================
        [OneTimeSetUp]
        public void GlobalSetup()
        {
            // Load settings (browser, report path, base URL, login)
            Settings = JsonFileReader.Load<TestSettings>("testsettings.json");

            // Resolve project root from bin/Debug/net8.0 to repo root
            var root = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

            // Build absolute report path from project root + config value
            var reportPath = Path.Combine(root, Settings.Report.Path);

            // Ensure the report directory exists
            var reportDir = Path.GetDirectoryName(reportPath);
            if (!string.IsNullOrEmpty(reportDir))
            {
                Directory.CreateDirectory(reportDir);
            }

            try
            {
                // Wire up ExtentReports with configured report file
                var htmlReporter = new ExtentSparkReporter(reportPath);
                htmlReporter.Config.DocumentTitle = Settings.Report.Title;
                htmlReporter.Config.ReportName = Settings.Report.Title;

                Extent = new ExtentReports();
                Extent.AttachReporter(htmlReporter);

                Console.WriteLine($"[INFO] Extent report path: {reportPath}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Extent setup failed: {e}");
                throw;
            }
        }

        // =======================
        // PER-TEST SETUP
        // =======================
        [SetUp]
        public void TestSetup()
        {
            // Create browser instance based on config (Chrome / Firefox, headless)
            Driver = CreateWebDriver();
            Driver.Manage().Window.Maximize();
            Driver.Manage().Timeouts().ImplicitWait =
                TimeSpan.FromSeconds(Settings.Browser.TimeoutSeconds);

            // Initialise navigation + pages with base URL
            Nav = new NavigationHelper(Driver, Settings.Environment.BaseUrl);
            LoginPage = new LoginPage(Driver, Nav);
            EducationPage = new EducationPage(Driver, Nav);
            CertificationPage = new CertificationPage(Driver, Nav);

            // Common login flow for every test
            LoginPage.OpenSignIn();
            LoginPage.Login(Settings.Login.Username, Settings.Login.Password);
            LoginPage.WaitUntilLoggedIn();

            // Optional per-test preconditions (overridden in derived test classes)
            BeforeEachTest();

            // Create Extent node for current test
            var testName = TestContext.CurrentContext.Test.Name;
            Test = Extent.CreateTest(HtmlSafe(testName));
        }

        private IWebDriver CreateWebDriver()
        {
            // Select browser type from config (default: Chrome)
            var type = Settings.Browser.Type?.ToLowerInvariant() ?? "chrome";
            if (type == "firefox")
            {
                var opts = new FirefoxOptions();
                if (Settings.Browser.Headless)
                    opts.AddArgument("--headless");
                return new FirefoxDriver(opts);
            }

            var ch = new ChromeOptions();
            if (Settings.Browser.Headless)
                ch.AddArgument("--headless=new");
            return new ChromeDriver(ch);
        }

        // Hook to allow test classes to run pre-test cleanup/setup
        protected virtual void BeforeEachTest()
        {
            // default: do nothing
        }

        // Hook to allow test classes to run post-test cleanup
        protected virtual void AfterEachTest()
        {
            // default: do nothing
        }

        // =======================
        // EDUCATION CLEANUP
        // =======================
        protected void PreCleanEducation()
        {
            try
            {
                // Ensure Education grid starts from a clean state
                EducationPage.OpenEducationTab();
                var before = EducationPage.GetRowCount();
                TestContext.WriteLine($"PRE-CLEAN: Found {before} rows");
                if (before > 0)
                {
                    EducationPage.DeleteAllEducation();
                    var after = EducationPage.GetRowCount();
                    TestContext.WriteLine($"AFTER CLEAN: {after} rows");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PreClean] {ex.Message}");
            }
        }

        protected void PostCleanEducation()
        {
            try
            {
                // Hard cleanup after test to remove any added rows
                EducationPage.OpenEducationTab();
                EducationPage.DeleteAllEducation();
                TestContext.WriteLine("POST-CLEAN complete");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PostClean] {ex.Message}");
            }
        }

        // =======================
        // CERTIFICATION CLEANUP
        // =======================
        protected void PreCleanCertification()
        {
            try
            {
                // Ensure Certification grid starts from a clean state
                CertificationPage.OpenCertificationTab();
                var before = CertificationPage.GetRowCount();
                TestContext.WriteLine($"PRE-CLEAN (Cert): Found {before} rows");
                if (before > 0)
                {
                    CertificationPage.DeleteAllCertification();
                    var after = CertificationPage.GetRowCount();
                    TestContext.WriteLine($"AFTER CLEAN (Cert): {after} rows");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PreCleanCertification] {ex.Message}");
            }
        }

        protected void PostCleanCertification()
        {
            try
            {
                // Hard cleanup after test to remove any added certification rows
                CertificationPage.OpenCertificationTab();
                CertificationPage.DeleteAllCertification();

                TestContext.WriteLine("POST-CLEAN (Cert) complete");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PostCleanCertification] {ex.Message}");
            }
        }

        // =======================
        // PER-TEST TEARDOWN
        // =======================
        [TearDown]
        public void TestTeardown()
        {
            var result = TestContext.CurrentContext.Result;
            var status = result.Outcome.Status;
            var message = result.Message;
            Driver.TryDismissAnyAlert();

            try
            {
                // Push collected LoggerHelper lines into the Extent test
                foreach (var line in GetLogs())
                    Test.Info(HtmlSafe(line));   // encode each log line
                ClearLogs();

                if (status == NUnit.Framework.Interfaces.TestStatus.Failed)
                {
                    var name = TestContext.CurrentContext.Test.Name;
                    var path = SaveScreenshot(name);

                    var safeMessage = HtmlSafe(message);   // encode failure message

                    if (!string.IsNullOrEmpty(path))
                    {
                        Test.Fail(safeMessage).AddScreenCaptureFromPath(path);
                    }
                    else
                    {
                        Test.Fail(safeMessage);
                    }
                }

                else
                {
                    // Mark test as passed when no failure
                    Test.Pass("Passed");
                    TestContext.WriteLine("Test passed (no screenshot)");
                }

                // Allow derived classes to run extra teardown (e.g. custom cleanup)
                AfterEachTest();
            }
            finally
            {
                // Always close and dispose the browser
                Driver?.Quit();
                Driver?.Dispose();
            }
        }

        // =======================
        // SAVE SCREENSHOT TO FILE
        // =======================
        protected string SaveScreenshot(string testName)
        {
            try
            {
                // Save screenshots under <project-root>/Screenshots
                var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
                var dir = Path.Combine(root, "Screenshots");
                Directory.CreateDirectory(dir);

                var safeName = MakeSafeFileName(testName);

                var file = Path.Combine(dir, $"{safeName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");

                var shot = ((ITakesScreenshot)Driver).GetScreenshot();
                File.WriteAllBytes(file, shot.AsByteArray);
                Console.WriteLine($"[SCREENSHOT] {file}");
                return file;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Screenshot failed: {ex.Message}");
                return string.Empty;
            }
        }

        // Normalise test names into valid file names for screenshots
        private string MakeSafeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Test";

            var safe = name;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                safe = safe.Replace(c, '_');
            }

            // Clean up a few extra characters often present in test names
            safe = safe.Replace("\"", "").Replace("(", "_").Replace(")", "_");

            if (string.IsNullOrWhiteSpace(safe))
                safe = "Test";

            return safe;
        }

        // =======================
        // GLOBAL TEARDOWN
        // =======================
        [OneTimeTearDown]
        public void GlobalTeardown()
        {
            try
            {
                // Flush ExtentReports buffer and write final HTML file
                Extent?.Flush();
                Console.WriteLine($"[INFO] Report flushed: {Settings?.Report?.Path}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Flush failed: {ex.Message}");
            }
        }
    }
}
