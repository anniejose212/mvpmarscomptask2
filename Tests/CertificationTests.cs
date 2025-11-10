using AventStack.ExtentReports;
using mars.nunit.json.Pages;
using mars_nunit_json.Hooks;
using mars_nunit_json.Support;
using mars_nunit_json.TestDataModel;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;

namespace mars_nunit_json.Tests
{
    [TestFixture]
    //[Parallelizable(ParallelScope.All)]
    public class CertificationTests : Base
    {
        protected override void BeforeEachTest()
        {
            PreCleanCertification();
        }

        protected override void AfterEachTest()
        {
            PostCleanCertification();
        }

        //private CertificationPage CertificationPage => new CertificationPage(Driver, Nav);

        // ======================
        // CERT-01 Certification tab loads empty state
        // ======================
        [Test]
        public void CertificationTab_LoadsEmptyState()
        {
            CertificationPage.OpenCertificationTab();

            var count = CertificationPage.GetRowCount();

            Assert.That(count, Is.EqualTo(0),
  "Expected certification grid to be empty on first load.");
        }

        // ======================
        // CERT-02 Add certification
        // ======================
        [Test]
        public void AddCertification_AddsRowAndShowsSuccessToast()
        {
            var path = TestDataPath.Resolve("AddCertification.json");
            var data = JsonFileReader.Load<List<CertificationRecord>>(path);
            var record = data[0];
            var toast = new ToastHelper(Driver);

            Info($"ADD CERT: {record.Certificate}, {record.From}, {record.Year.ToString()}");

            CertificationPage.OpenCertificationTab();
            CertificationPage.OpenForm();
            CertificationPage.FillCertificationForm(record);
            CertificationPage.ClickAddButton();

            var successToast = toast.GetSuccessToastText();
            Info($"Success toast: {successToast}");
            TestContext.Progress.WriteLine($"Success toast: {successToast}");

            Assert.Multiple(() =>
            {
                Assert.That(successToast, Is.Not.Null.And.Not.Empty.And.Contains("has been added"),
                        "Expected specific success message after add.");
                AssertCertificationPresence(record, shouldExist: true);
            });


            var screenshotPath = SaveScreenshot("AddCertificationAddsRowAndShowsSuccessToast");
            if (!string.IsNullOrEmpty(screenshotPath))
                Test.AddScreenCaptureFromPath(screenshotPath);


            Test.Log(Status.Info, "Screenshot attached after successful add for demo purposes.");
        }

        // ======================
        // CERT-03 Prevent duplicate certification
        // ======================
        [Test]
        public void PreventDuplicateCertification_ShowsErrorAndKeepsSingleRow()
        {
            var path = TestDataPath.Resolve("DuplicateDataCertification.json");
            var data = JsonFileReader.Load<List<CertificationRecord>>(path);
            var record = data[0];
            var toast = new ToastHelper(Driver);

            Info($"DUPLICATE CERT: {record.Certificate}, {record.From}, {record.Year.ToString()}");

            CertificationPage.OpenCertificationTab();

            // First add
            CertificationPage.OpenForm();
            CertificationPage.FillCertificationForm(record);
            CertificationPage.ClickAddButton();
            Thread.Sleep(1500);
            toast.CloseToast();

            // Duplicate add
            CertificationPage.OpenForm();
            CertificationPage.FillCertificationForm(record);
            CertificationPage.ClickAddButton();


            var errorToast = toast.GetErrorToastText();
            Info($"Error toast: {errorToast}");
            TestContext.Progress.WriteLine($"Error toast: {errorToast}");

            Assert.Multiple(() =>
            {
                Assert.That(errorToast, Is.Not.Null.And.Not.Empty.And.Contains("already exist"));

                AssertCertificationPresence(record, shouldExist: true);
                AssertCertificationCount(record, expectedCount: 1);
            });
        }

        // ======================
        // CERT-04 Edit certification
        // ======================
        [Test]
        public void EditCertification_ReplacesOriginalRowWithUpdatedRow()
        {
            var path = TestDataPath.Resolve("EditCertification.json");
            var data = JsonFileReader.Load<List<CertificationRecord>>(path);
            var original = data[0];
            var updated = data[1];
            var toast = new ToastHelper(Driver);

            Info($"ORIGINAL CERT: {original.Certificate}, {original.From}, {original.Year}");
            Info($"UPDATED  CERT: {updated.Certificate},  {updated.From},  {updated.Year}");

            CertificationPage.OpenCertificationTab();
            CertificationPage.OpenForm();
            CertificationPage.FillCertificationForm(original);
            CertificationPage.ClickAddButton();
            Thread.Sleep(1500);
            toast.CloseToast();
            CertificationPage.EditFirstCertification(updated);
            CertificationPage.ClickUpdateButton();

            var successToast = toast.GetSuccessToastText();

            Info($"Success toast: {successToast}");
            TestContext.Progress.WriteLine($"Success toast: {successToast}");

            Assert.Multiple(() =>
            {
                Assert.That(successToast, Is.Not.Null.And.Not.Empty.And.Contains("been updated"),
                    "Expected success toast after update.");
                AssertCertificationCount(updated, expectedCount: 1);
                AssertCertificationPresence(original, shouldExist: false);
            });
        }

        // ======================
        // CERT-05 Delete a certification row
        // ======================
        [Test]
        public void DeleteCertificationRow_RemovesRowAndShowsSuccessToast()
        {
            var path = TestDataPath.Resolve("DeleteCertification.json");
            var data = JsonFileReader.Load<List<CertificationRecord>>(path);
            var record = data[0];
            var toast = new ToastHelper(Driver);

            Info($"DELETE CERT: {record.Certificate}, {record.From}, {record.Year.ToString()}");

            CertificationPage.OpenCertificationTab();
            CertificationPage.OpenForm();
            CertificationPage.FillCertificationForm(record);
            CertificationPage.ClickAddButton();
            Thread.Sleep(1500);
            toast.CloseToast();
            CertificationPage.DeleteCertification(record);


            var successToast = toast.GetSuccessToastText();

            Info($"Success toast: {successToast}");
            TestContext.Progress.WriteLine($"Success toast: {successToast}");

            Assert.Multiple(() =>
            {
                Assert.That(successToast, Is.Not.Null.And.Not.Empty.And.Contains("deleted").IgnoreCase, "Expected non-empty success toast message mentioning deletion.");

                AssertCertificationPresence(record, shouldExist: false);
            });
        }

        // ======================
        // CERT-06 Cancel add
        // ======================
        [Test]
        public void CancelAdd_DoesNotPersistCertificationRow()
        {
            var path = TestDataPath.Resolve("CancelAddCertification.json");
            var data = JsonFileReader.Load<List<CertificationRecord>>(path);
            var record = data[0];

            Info($"CANCEL ADD CERT: {record.Certificate}, {record.From}, {record.Year.ToString()}");

            CertificationPage.OpenCertificationTab();
            CertificationPage.OpenForm();
            CertificationPage.FillCertificationForm(record);
            CertificationPage.CancelCertificationAdd();

            AssertCertificationPresence(record, shouldExist: false);
        }

        // ======================
        // CERT-07 Required field validation blocks add
        // ======================
        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void RequiredFieldValidation_BlocksAddAndShowsError(int index)
        {
            var path = TestDataPath.Resolve("CertificationRequiredFieldValidation.json");
            var data = JsonFileReader.Load<List<CertificationRecord>>(path);
            var record = data[index];
            var toast = new ToastHelper(Driver);

            Info($"REQ-VALID CERT Index {index}: {record.Certificate}, {record.From}, {record.Year.ToString()}");

            CertificationPage.OpenCertificationTab();
            CertificationPage.OpenForm();
            CertificationPage.FillCertificationForm(record);
            CertificationPage.ClickAddButton();


            var errorToast = toast.GetErrorToastText();
            Info($"Error toast: {errorToast}");
            TestContext.Progress.WriteLine($"Success toast: {errorToast}");

            Assert.Multiple(() =>
            {
                Assert.That(errorToast, Is.Not.Null.And.Not.Empty.And.Contains("Please enter"),
                    "Expected error message that all fields are required.");
                AssertCertificationPresence(record, shouldExist: false);
            });
        }

        // ======================
        // CERT-08 Cancel edit
        // ======================
        [Test]
        public void CancelEdit_ResultsInOneRow()
        {
            var path = TestDataPath.Resolve("CancelUpdateCertification.json");
            var data = JsonFileReader.Load<List<CertificationRecord>>(path);
            var original = data[0];
            var updated = data[1];

            Info($"CANCEL EDIT CERT ORIGINAL: {original.Certificate}, {original.From}, {original.Year}");
            Info($"CANCEL EDIT CERT UPDATED: {updated.Certificate},  {updated.From},  {updated.Year}");

            CertificationPage.OpenCertificationTab();
            CertificationPage.OpenForm();
            CertificationPage.FillCertificationForm(original);
            CertificationPage.ClickAddButton();

            CertificationPage.EditFirstCertification(updated);
            CertificationPage.CancelCertificationAdd();

            Assert.Multiple(() =>
            {
                AssertCertificationPresence(original, shouldExist: true);
                AssertCertificationPresence(updated, shouldExist: false);
            });
        }

        // ======================
        // CERT-09 Double-submit on Add inserts once
        // ======================
        [Test]
        public void DoubleSubmitOnAdd_InsertsOnceAndShowsSuccess()
        {
            var path = TestDataPath.Resolve("DoubleSubmitAddCertification.json");
            var data = JsonFileReader.Load<List<CertificationRecord>>(path);
            var record = data[0];
            var toast = new ToastHelper(Driver);

            Info($"DOUBLE ADD CERT: {record.Certificate}, {record.From}, {record.Year.ToString()}");

            CertificationPage.OpenCertificationTab();
            CertificationPage.OpenForm();
            CertificationPage.FillCertificationForm(record);
            CertificationPage.DoubleClickAddButton();

            var successToast = toast.GetSuccessToastText();

            Info($"Success toast: {successToast}");
            TestContext.Progress.WriteLine($"Success toast: {successToast}");

            Assert.Multiple(() =>
            {
                Assert.That(successToast, Is.Not.Null.And.Not.Empty.And.Contains("has been added"),
                    "Expected success toast after double-submit add.");
                AssertCertificationPresence(record, shouldExist: true);
            });
        }

        // ======================
        // CERT-10 Double-submit on Update updates once
        // ======================
        [Test]
        public void DoubleSubmitOnUpdate_UpdatesOnceAndShowsSuccess()
        {
            var path = TestDataPath.Resolve("DoubleSubmitUpdateCertification.json");
            var data = JsonFileReader.Load<List<CertificationRecord>>(path);
            var original = data[0];
            var updated = data[1];
            var toast = new ToastHelper(Driver);

            Info($"DOUBLE UPDATE CERT ORIGINAL: {original.Certificate}, {original.From}, {original.Year}");
            Info($"DOUBLE UPDATE CERT UPDATED:  {updated.Certificate},  {updated.From},  {updated.Year}");

            CertificationPage.OpenCertificationTab();
            CertificationPage.OpenForm();
            CertificationPage.FillCertificationForm(original);
            CertificationPage.ClickAddButton();
            Thread.Sleep(500);
            toast.CloseToast();
            CertificationPage.EditFirstCertification(updated);
            CertificationPage.DoubleClickUpdateButton();

            var successToast = toast.GetSuccessToastText();

            Info($"Success toast: {successToast}");
            TestContext.Progress.WriteLine($"Success toast: {successToast}");
            var updatedCount = CertificationPage.CountCertificationRows(updated);

            Info($"After double update | UpdatedCount: {updatedCount} | Toast: {successToast}");

            Assert.Multiple(() =>
            {
                Assert.That(successToast, Is.Not.Null.And.Not.Empty.And.Contains("as been updated"),
                    "Expected success toast after double-submit update.");
                AssertCertificationPresence(original, shouldExist: false);
                AssertCertificationPresence(updated, shouldExist: true);
            });
        }

        // ======================
        // CERT-INV Robustness / invalid input
        // ======================
        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
       

        public void Robustness_InvalidCertificationInput_ShouldBeRejected(int index)
        {
            var path = TestDataPath.Resolve("CertificationRobustness.json");
            var data = JsonFileReader.Load<List<CertificationRecord>>(path);
            var toast = new ToastHelper(Driver);
            var record = data[index];
            Info($"[Cert Robustness] Case #: {record.Certificate}, {record.From}, {record.Year.ToString()}");


            CertificationPage.OpenCertificationTab();
            CertificationPage.OpenForm();
            CertificationPage.FillCertificationForm(record);
            CertificationPage.ClickAddButton();


            var alert = Driver.TryGetAlert(2);
            if (alert != null)
            {
                var text = alert.Text ?? string.Empty;
                TestContext.WriteLine($"[ALERT DETECTED] Education alert text: '{text}'");
                alert.Accept();
            }
            else
            {
                TestContext.WriteLine("[NO ALERT DETECTED]");
            }


            toast.PrintAllToasts();

            AssertCertificationPresence(record, shouldExist: false);


        }

        // ======================
        // CERT Security – XSS
        // ======================
        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Security_XssInput_ShouldBeRejected_ForCertification(int index)
        {
            var path = TestDataPath.Resolve("CertificationSecurityXss.json");
            var data = JsonFileReader.Load<List<CertificationRecord>>(path);
            var record = data[index];
            var toast = new ToastHelper(Driver);

            Info($"[Cert Security XSS] {record.Certificate}, {record.From}, {record.Year.ToString()}");

            CertificationPage.OpenCertificationTab();
            CertificationPage.OpenForm();
            CertificationPage.FillCertificationForm(record);
            CertificationPage.ClickAddButton();

            var alert = Driver.TryGetAlert(2);
            if (alert != null)
            {
                var text = alert.Text ?? string.Empty;
                TestContext.WriteLine($"[ALERT DETECTED] Certification alert text: '{text}'");
                alert.Accept();
            }
            else
            {
                TestContext.WriteLine("[NO ALERT DETECTED]");
            }

            toast.PrintAllToasts();

            AssertCertificationPresence(record, shouldExist: false);

        }

        // ======================
        // CERT Security – SQL-like
        // ======================
        [Test]
        [TestCase(0)]
        [TestCase(1)]

        public void Security_SqlLikeInput_ShouldBeRejected_ForCertification(int index)
        {
            var path = TestDataPath.Resolve("CertificationSecuritySqlLike.json");
            var data = JsonFileReader.Load<List<CertificationRecord>>(path);
            var record = data[index];
            var toast = new ToastHelper(Driver);

            Info($"[Cert Security SQL] {record.Certificate}, {record.From}, {record.Year.ToString()}");

            CertificationPage.OpenCertificationTab();
            CertificationPage.OpenForm();
            CertificationPage.FillCertificationForm(record);
            CertificationPage.ClickAddButton();

            var alert = Driver.TryGetAlert(2);
            if (alert != null)
            {
                var text = alert.Text ?? string.Empty;
                alert.Accept();
                Assert.Fail($"Unexpected alert triggered by SQL-like input: '{text}'. Input should be treated as plain text.");
            }

            toast.PrintAllToasts();
            AssertCertificationPresence(record, shouldExist: false);

        }

        // ======================
        // CERT-INV Max length 255 accepted
        // ======================
        [Test]
        public void MaxLength255_Accepted_AndGridValueLengthIs255_ForCertification()
        {
            var path = TestDataPath.Resolve("CertificationMaxLength255.json");
            var data = JsonFileReader.Load<List<CertificationRecord>>(path);
            var record = data[0];
            var toast = new ToastHelper(Driver);

            Info($"CERT MAXLEN 255: {record.Certificate}, {record.From}, {record.Year.ToString()}");

            CertificationPage.OpenCertificationTab();
            CertificationPage.OpenForm();
            CertificationPage.FillCertificationForm(record);
            CertificationPage.ClickAddButton();

            var successToast = toast.GetSuccessToastText();
            var rows = CertificationPage.GetCertificationRows();
            var certificate = rows.Count > 0 ? rows[0].Certificate ?? string.Empty : string.Empty;
            var length = certificate.Length;

            Info($"Grid Certificate length: {length} | Toast: {successToast}");

            Assert.Multiple(() =>
            {
                Assert.That(successToast, Is.Not.Null.And.Not.Empty,
                    "Expected success toast for certification max length 255 scenario.");

                Assert.That(length, Is.EqualTo(255),
                    "Expected Certificate grid value length to be exactly 255.");
            });
        }

        // ======================
        // CERT Year boundary
        // ======================
        [Test]
        [TestCase(0)] // min year
        [TestCase(1)] // max year
        public void CertificationYear_BoundaryValues_Accepted(int index)
        {
            var path = TestDataPath.Resolve("CertificationYearBoundary.json");
            var data = JsonFileReader.Load<List<CertificationRecord>>(path);
            var record = data[index];
            var toast = new ToastHelper(Driver);

            Info($"CERT YEAR-BOUNDARY Index {index}: {record.Certificate}, {record.From}, {record.Year.ToString()}");

            CertificationPage.OpenCertificationTab();
            CertificationPage.OpenForm();
            CertificationPage.FillCertificationForm(record);
            CertificationPage.ClickAddButton();

            var successToast = toast.GetSuccessToastText();
            var matchCount = CertificationPage.CountCertificationRows(record);

            Info($"Index {index} | MatchCount: {matchCount} | Toast: {successToast}");

            Assert.Multiple(() =>
            {
                Assert.That(successToast, Is.Not.Null.And.Not.Empty,
                    "Expected success toast for certification boundary year scenario.");

                Assert.That(matchCount, Is.EqualTo(1),
                    "Boundary year certification record should be saved in the grid exactly once.");
            });
        }

        // ======================
        // CERT Dropdown placeholder – Year
        // ======================
        [Test]
        [TestCase(0)]
        public void CertificationDropdownPlaceholder_Selections_ShouldBeRejected(int index)
        {
            var path = TestDataPath.Resolve("CertificationDropdownPlaceholder.json");
            var data = JsonFileReader.Load<List<CertificationRecord>>(path);
            var record = data[index];
            var toast = new ToastHelper(Driver);

            Info($"CERT DROPDOWN-PLACEHOLDER Index {index}: {record.Certificate}, {record.From}, {record.Year.ToString()}");

            CertificationPage.OpenCertificationTab();
            CertificationPage.OpenForm();
            CertificationPage.FillCertificationForm(record);
            CertificationPage.ClickAddButton();


            var errorToast = toast.GetErrorToastText();
            var successToast = string.Empty;
            if (string.IsNullOrEmpty(errorToast))
            {
                successToast = toast.GetSuccessToastText();
            }

            var rowCount = CertificationPage.CountCertificationRows(record);

            Info($"Index {index} | Success: {successToast} | Error: {errorToast} | RowCount: {rowCount}");

            Assert.Multiple(() =>
            {
                Assert.That(successToast, Is.Null.Or.Empty,
                    "System must NOT accept placeholder year selections (no success toast).");

                Assert.That(errorToast ?? string.Empty,
                    Does.Contain("Please enter").IgnoreCase,
                    "System SHOULD reject placeholder year selections (error toast expected).");

                AssertCertificationPresence(record, shouldExist: false);
            });
        }

        // ===== Helpers =====

        private void AssertCertificationPresence(CertificationRecord record, bool shouldExist)
        {
            int count = CertificationPage.CountCertificationRows(record);
            string details = CertificationPage.GetCertificationDetails();

            Info($"ASSERT CERT PRESENCE | ShouldExist={shouldExist} | MatchCount={count} | " +
                 $"Record: {record.Certificate} | {record.From} | {record.Year.ToString()}");
            Info($"Table snapshot: [{details}]");

            if (shouldExist)
            {
                Assert.That(count, Is.GreaterThan(0),
                    $"Missing certification row: \"{record.Certificate}\" | \"{record.From}\" | " +
                    $"\"{record.Year.ToString()}\". Table: [{details}]");
            }
            else
            {
                Assert.That(count, Is.EqualTo(0),
                    $"Unexpected certification row found {count} time(s): \"{record.Certificate}\" | " +
                    $"\"{record.From}\" | \"{record.Year.ToString()}\". Table: [{details}]");
            }
        }

        private void AssertCertificationCount(CertificationRecord record, int expectedCount)
        {
            var count = CertificationPage.CountCertificationRows(record);
            var details = CertificationPage.GetCertificationDetails();

            Assert.That(count, Is.EqualTo(expectedCount),
                $"Expected {expectedCount} matching row(s) for: " +
                $"\"{record.Certificate}\" | \"{record.From}\" | \"{record.Year.ToString()}\". Actual: {count}. " +
                $"Table: [{details}]");
        }
    }
}
