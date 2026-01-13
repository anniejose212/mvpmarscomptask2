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

    public class EducationTests : Base
    {

        protected override void BeforeEachTest()
        {
            PreCleanEducation();
        }

        protected override void AfterEachTest()
        {
            PostCleanEducation();
        }

        // ======================
        // EDU-01 Education tab loads empty state
        // ======================
        [Test]
        public void EducationTab_LoadsEmptyState()
        {
            EducationPage.OpenEducationTab();

            var count = EducationPage.GetRowCount();

            Assert.That(count, Is.EqualTo(0), "Expected education grid to be empty on first load.");
        }

        // ======================
        // EDU-02 Add education
        // ======================
        [Test]
        public void AddEducation_AddsRowAndShowsSuccessToast()
        {
            var path = TestDataPath.Resolve("AddEducation.json");
            var data = JsonFileReader.Load<List<EducationRecord>>(path);
            var record = data[0];
            var toast = new ToastHelper(Driver);

            Info($"ADD: {record.University}, {record.Country}, {record.Title}, {record.Degree}, {record.GraduationYear.ToString()}");

            EducationPage.OpenEducationTab();
            EducationPage.OpenForm();
            EducationPage.FillEducationForm(record);
            EducationPage.ClickAddButton();

            var successToast = toast.GetSuccessToastText();

            Info($"Success toast: {successToast}");
            TestContext.Progress.WriteLine($"Success toast: {successToast}");

            Assert.Multiple(() =>
            {
                Assert.That(successToast, Is.Not.Null.And.Not.Empty.And.Contains("has been added"),
    "Expected specific success message after add.");
                AssertEducationPresence(record, shouldExist: true);
            });
        }

        // ======================
        // EDU-03 Prevent duplicate education
        // ======================
        [Test]
        public void PreventDuplicateEducation_ShowsErrorAndKeepsSingleRow()
        {
            var path = TestDataPath.Resolve("DuplicateDataEducation.json");
            var data = JsonFileReader.Load<List<EducationRecord>>(path);
            var record = data[0];
            var toast = new ToastHelper(Driver);

            
            Info($"DUPLICATE: {record.University}, {record.Country}, {record.Title}, {record.Degree}, {record.GraduationYear.ToString()}");

            EducationPage.OpenEducationTab();


            EducationPage.OpenForm();
            EducationPage.FillEducationForm(record);
            EducationPage.ClickAddButton();
            Thread.Sleep(1500);
            toast.CloseToast();

            // Duplicate add
            EducationPage.OpenForm();
            EducationPage.FillEducationForm(record);
            EducationPage.ClickAddButton();
            var errorToast = toast.GetErrorToastText();
            Info($"Error toast: {errorToast}");
            TestContext.Progress.WriteLine($"Error toast: {errorToast}");

            Assert.Multiple(() =>
            {
                Assert.That(errorToast, Is.Not.Null.And.Not.Empty.And.Contains("This information is already exist"),
                    "Expected specific error message for duplicate education.");

                AssertEducationPresence(record, shouldExist: true);
                AssertEducationCount(record, expectedCount: 1);

            });
        }



        // ======================
        // EDU-04 Edit education
        // ======================
        [Test]
        public void EditEducation_ReplacesOriginalRowWithUpdatedRow()
        {
            var path = TestDataPath.Resolve("EditEducation.json");
            var data = JsonFileReader.Load<List<EducationRecord>>(path);
            var original = data[0];
            var updated = data[1];
            var toast = new ToastHelper(Driver);

            Info($"ORIGINAL: {original.University}, {original.Country}, {original.Title}, {original.Degree}, {original.GraduationYear}");
            Info($"UPDATED: {updated.University}, {updated.Country}, {updated.Title}, {updated.Degree}, {updated.GraduationYear}");

            EducationPage.OpenEducationTab();
            EducationPage.OpenForm();
            EducationPage.FillEducationForm(original);
            EducationPage.ClickAddButton();
            Thread.Sleep(1500);
            toast.CloseToast();
            EducationPage.EditFirstEducation(updated);
            EducationPage.ClickUpdateButton();

            var successToast = toast.GetSuccessToastText();

            Info($"Success toast: {successToast}");
            TestContext.Progress.WriteLine($"Success toast: {successToast}");

            Assert.Multiple(() =>
            {
                Assert.That(successToast, Is.Not.Null.And.Not.Empty.And.Contains("been updated"), "Expected success toast after update.");
                AssertEducationCount(updated, expectedCount: 1);
                AssertEducationPresence(original, shouldExist: false);
            });
        }


        // ======================
        // EDU-05 Delete an education row
        // ======================
        [Test]
        public void DeleteEducationRow_RemovesRowAndShowsSuccessToast()
        {
            var path = TestDataPath.Resolve("DeleteEducation.json");
            var data = JsonFileReader.Load<List<EducationRecord>>(path);
            var record = data[0];
            var toast = new ToastHelper(Driver);

            Info($"DELETE: {record.University}, {record.Country}, {record.Title}, {record.Degree}, {record.GraduationYear.ToString().ToString()}");

            EducationPage.OpenEducationTab();
            EducationPage.OpenForm();
            EducationPage.FillEducationForm(record);
            EducationPage.ClickAddButton();
            Thread.Sleep(1500);
            toast.CloseToast();
            EducationPage.DeleteEducation(record);

            var successToast = toast.GetSuccessToastText();
            
            Info($"Success toast: {successToast}");
            TestContext.Progress.WriteLine($"Success toast: {successToast}");

            Assert.Multiple(() =>
            {
                Assert.That(successToast, Is.Not.Null.And.Not.Empty.And.Contains("removed").IgnoreCase, "Expected delete toast message to mention removal.");
                AssertEducationPresence(record, shouldExist: false);
               
            });
        }
        // ======================
        // EDU-06 Cancel add
        // ======================
        [Test]
        public void CancelAdd_DoesNotPersistRow()
        {
            var path = TestDataPath.Resolve("CancelAddEducation.json");
            var data = JsonFileReader.Load<List<EducationRecord>>(path);
            var record = data[0];

            Info($"CANCEL ADD: {record.University}, {record.Country}, {record.Title}, {record.Degree}, {record.GraduationYear.ToString()}");

            EducationPage.OpenEducationTab();
            EducationPage.OpenForm();
            EducationPage.FillEducationForm(record);
            EducationPage.CancelEducationAdd();

            AssertEducationPresence(record, shouldExist: false);
        }

        // ======================
        // EDU-07 Required field validation blocks add
        // ======================
        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void RequiredFieldValidation_BlocksAddAndShowsError(int index)
        {
            var path = TestDataPath.Resolve("RequiredFieldValidationEducation.json");
            var data = JsonFileReader.Load<List<EducationRecord>>(path);
            var record = data[index];
            var toast = new ToastHelper(Driver);

            Info($"REQ-VALID Index {index}: {record.University}, {record.Country}, {record.Title}, {record.Degree}, {record.GraduationYear.ToString()}");

            EducationPage.OpenEducationTab();
            EducationPage.OpenForm();
            EducationPage.FillEducationForm(record);
            EducationPage.ClickAddButton();

            var errorToast = toast.GetErrorToastText();
            Info($"Error toast: {errorToast}");
            TestContext.Progress.WriteLine($"Success toast: {errorToast}");

            Assert.Multiple(() =>
            {
                Assert.That(errorToast, Is.Not.Null.And.Not.Empty.And.Contains("Please enter"),
                  "Expected error message that all fields are required.");

                AssertEducationPresence(record, shouldExist: false);
            });
        }

        // ======================
        // EDU-08 Cancel edit 
        // ======================
        [Test]
        public void CancelEdit_ResultsInOneRow()
        {
            var path = TestDataPath.Resolve("CancelUpdateEducation.json");
            var data = JsonFileReader.Load<List<EducationRecord>>(path);
            var original = data[0];
            var updated = data[1];

            Info($"CANCEL EDIT ORIGINAL: {original.University}, {original.Country}, {original.Title}, {original.Degree}, {original.GraduationYear}");
            Info($"CANCEL EDIT UPDATED: {updated.University}, {updated.Country}, {updated.Title}, {updated.Degree}, {updated.GraduationYear}");

            EducationPage.OpenEducationTab();
            EducationPage.OpenForm();
            EducationPage.FillEducationForm(original);
            EducationPage.ClickAddButton();

            EducationPage.EditFirstEducation(updated);
            EducationPage.CancelEducationAdd();

            Assert.Multiple(() =>
                {   AssertEducationPresence(updated, shouldExist: false);
                    AssertEducationPresence(original, shouldExist: true);
             
                });
            
        }

        // ======================
        // EDU-09 Double-submit on Add inserts once
        // ======================
        [Test]
        public void DoubleSubmitOnAdd_InsertsOnceAndShowsSuccess()
        {
            var path = TestDataPath.Resolve("DoubleSubmitAddEducation.json");
            var data = JsonFileReader.Load<List<EducationRecord>>(path);
            var record = data[0];
            var toast = new ToastHelper(Driver);

            Info($"DOUBLE ADD: {record.University}, {record.Country}, {record.Title}, {record.Degree}, {record.GraduationYear.ToString()}");

            EducationPage.OpenEducationTab();
            EducationPage.OpenForm();
            EducationPage.FillEducationForm(record);
            EducationPage.DoubleClickAddButton();

            var successToast = toast.GetSuccessToastText();

            Info($"Success toast: {successToast}");
            TestContext.Progress.WriteLine($"Success toast: {successToast}");

            Assert.Multiple(() =>
            {
                Assert.That(successToast, Is.Not.Null.And.Not.Empty.And.Contains("has been added"));
                AssertEducationPresence(record, shouldExist: true);
            });
        }

        // ======================
        // EDU-10 Double-submit on Update inserts once
        // ======================
        [Test]
        public void DoubleSubmitOnUpdate_UpdatesOnceAndShowsSuccess()
        {
            var path = TestDataPath.Resolve("DoubleSubmitUpdateEducation.json");
            var data = JsonFileReader.Load<List<EducationRecord>>(path);
            var original = data[0];
            var updated = data[1];
            var toast = new ToastHelper(Driver);

            Info($"DOUBLE UPDATE ORIGINAL: {original.University}, {original.Country}, {original.Title}, {original.Degree}, {original.GraduationYear}");
            Info($"DOUBLE UPDATE UPDATED: {updated.University}, {updated.Country}, {updated.Title}, {updated.Degree}, {updated.GraduationYear}");

            EducationPage.OpenEducationTab();
            EducationPage.OpenForm();
            EducationPage.FillEducationForm(original);
            EducationPage.ClickAddButton();
            Thread.Sleep(1500);
            toast.CloseToast();
            EducationPage.EditFirstEducation(updated);
            EducationPage.DoubleClickUpdateButton();

            var successToast = toast.GetSuccessToastText();
            var updatedCount = EducationPage.CountEducationRows(updated);

            Info($"After double update | UpdatedCount: {updatedCount} | Toast: {successToast}");

            Assert.Multiple(() =>
            {
                Assert.That(successToast, Does.Contain("as been updated"));

                AssertEducationPresence(original, shouldExist: false);
            });
        }


        // EDU-11 <Dropdown> dropdown shows options and placeholder
        [Test]
        [TestCase("Country")]
        [TestCase("Title")]
        [TestCase("Year")]
        public void Dropdown_ShowsPlaceholderAndOptions(string dropdown)
        {
            EducationPage.OpenEducationTab();
            EducationPage.OpenForm();

            var select = EducationPage.GetSelectElement(dropdown);
            var options = select.Options;


            var placeholder = options.Count > 0 ? options[0].Text.Trim() : string.Empty;

            Info($"Dropdown {dropdown} | OptionCount: {options.Count} | Placeholder: {placeholder}");

            Assert.That(placeholder, Is.Not.Null.And.Not.Empty, $"Expected placeholder for {dropdown} dropdown.");
            Assert.That(options.Count, Is.GreaterThan(1), $"Expected options for {dropdown} dropdown.");
        }

        // ======================
        // EDU-INV Invalid or boundary input behaviour is inconsistent
        // ======================
        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void Robustness_WhitespaceOnly_ShouldBeRejected(int index)
        {
            var path = TestDataPath.Resolve("EducationRobustness.json");
            var data = JsonFileReader.Load<List<EducationRecord>>(path);
            var record = data[index];
            var toast = new ToastHelper(Driver);

            Info($"[ROBUSTNESS] Index {index} | Data: {record.University}, {record.Country}, {record.Title}, {record.Degree}, {record.GraduationYear.ToString()}");

            EducationPage.OpenEducationTab();
            EducationPage.OpenForm();
            EducationPage.FillEducationForm(record);
            EducationPage.ClickAddButton();

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
            AssertEducationPresence(record, shouldExist: false);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Security_XssInput_ShouldBeRejected(int index)
        {
            var path = TestDataPath.Resolve("EducationSecurityXss.json");
            var data = JsonFileReader.Load<List<EducationRecord>>(path);
            var record = data[index];
            var toast = new ToastHelper(Driver);

            Info($"[SECURITY XSS] Index {index} | {record.University}, {record.Country}, {record.Title}, {record.Degree}, {record.GraduationYear.ToString()}");

            EducationPage.OpenEducationTab();
            EducationPage.OpenForm();
            EducationPage.FillEducationForm(record);
            EducationPage.ClickAddButton();

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
            AssertEducationPresence(record, shouldExist: false);
        }


        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Security_SqlLikeInput_ShouldBeRejected(int index)
        {
            var path = TestDataPath.Resolve("EducationSecuritySqlLike.json");
            var data = JsonFileReader.Load<List<EducationRecord>>(path);
            var record = data[index];
            var toast = new ToastHelper(Driver);

            Info($"[SECURITY SQL-LIKE] Index {index} | {record.University}, {record.Country}, {record.Title}, {record.Degree}, {record.GraduationYear.ToString()}");

            EducationPage.OpenEducationTab();
            EducationPage.OpenForm();
            EducationPage.FillEducationForm(record);
            EducationPage.ClickAddButton();

            var alert = Driver.TryGetAlert(2);
            if (alert != null)
            {
                var text = alert.Text ?? string.Empty;
                alert.Accept();
                Assert.Fail($"Unexpected alert triggered by SQL-like input: '{text}'. Input should be treated as plain text.");
            }

            toast.PrintAllToasts();

            AssertEducationPresence(record, shouldExist: false);
      
        }

        // ======================
        // EDU-INV Max length 255 accepted exactly
        // ======================
        [Test]
        public void MaxLength255_Accepted_AndGridValueLengthIs255()
        {
            var path = TestDataPath.Resolve("EducationMaxLength255.json");
            var data = JsonFileReader.Load<List<EducationRecord>>(path);
            var record = data[0];
            var toast = new ToastHelper(Driver);

            Info($"MAXLEN 255: {record.University}, {record.Country}, {record.Title}, {record.Degree}, {record.GraduationYear.ToString()}");

            EducationPage.OpenEducationTab();
            EducationPage.OpenForm();
            EducationPage.FillEducationForm(record);
            EducationPage.ClickAddButton();

            var successToast = toast.GetSuccessToastText();
            var rows = EducationPage.GetEducationRows();
            var university = rows.Count > 0 ? rows[0].University ?? string.Empty : string.Empty;
            var length = university.Length;

            Info($"Grid University length: {length} | Toast: {successToast}");

            Assert.Multiple(() =>
            {
                Assert.That(successToast, Is.Not.Null.And.Not.Empty,
                    "Expected success toast for max length 255 scenario.");

                Assert.That(length, Is.EqualTo(255),
                    "Expected University grid value length to be exactly 255.");
            });
        }

        // ======================
        // EDU Year boundary
        // ======================

        [Test]
        [TestCase(0)] // min year
        [TestCase(1)] // max year
        public void GraduationYear_BoundaryValues_Accepted(int index)
        {
            var path = TestDataPath.Resolve("EducationYearBoundary.json");
            var data = JsonFileReader.Load<List<EducationRecord>>(path);
            var record = data[index];
            var toast = new ToastHelper(Driver);

            Info($"YEAR-BOUNDARY Index {index}: {record.University}, {record.Country}, {record.Title}, {record.Degree}, {record.GraduationYear.ToString()}");

            EducationPage.OpenEducationTab();
            EducationPage.OpenForm();
            EducationPage.FillEducationForm(record);
            EducationPage.ClickAddButton();

            var successToast = toast.GetSuccessToastText();
            var matchCount = EducationPage.CountEducationRows(record);

            Info($"Index {index} | MatchCount: {matchCount} | Toast: {successToast}");

            Assert.Multiple(() =>
            {
                Assert.That(successToast, Is.Not.Null.And.Not.Empty,
                    "Expected success toast for boundary year scenario.");

               
                Assert.That(matchCount, Is.EqualTo(1),
                    "Boundary year record should be saved in the grid exactly once.");

                
            });
        }

        // ======================
        // CERT Dropdown placeholder – Year
        // ======================

        [Test]
        [TestCase(0)] // Country placeholder
        [TestCase(1)] // Title placeholder
        [TestCase(2)] // Year placeholder
        public void DropdownPlaceholder_Selections_ShouldBeRejected(int index)
        {
            var path = TestDataPath.Resolve("EducationDropdownPlaceholder.json");
            var data = JsonFileReader.Load<List<EducationRecord>>(path);
            var record = data[index];
            var toast = new ToastHelper(Driver);

            Info($"DROPDOWN-PLACEHOLDER Index {index}: {record.University}, {record.Country}, {record.Title}, {record.Degree}, {record.GraduationYear.ToString()}");

            EducationPage.OpenEducationTab();
            EducationPage.OpenForm();
            EducationPage.FillEducationForm(record);
            EducationPage.ClickAddButton();

            // Error-first for invalid scenarios
            var errorToast = toast.GetErrorToastText();
            var successToast = string.Empty;
            if (string.IsNullOrEmpty(errorToast))
            {
                successToast = toast.GetSuccessToastText();
            }

            Info($"Index {index} | Success: {successToast} | Error: {errorToast}");

            Assert.Multiple(() =>
            {
                Assert.That(successToast, Is.Null.Or.Empty,
                    "System must NOT accept placeholder selections (no success toast).");

                Assert.That(errorToast ?? string.Empty,
                    Does.Contain("Please enter").IgnoreCase,
                    "System SHOULD reject placeholder selections (error toast expected).");

                AssertEducationPresence(record, shouldExist: false);
            });
        }

        // ===== Helpers =====

       
        private void AssertEducationPresence(EducationRecord record, bool shouldExist)
        {
            int count = EducationPage.CountEducationRows(record);
            string details = EducationPage.GetEducationDetails();

            Info($"ASSERT PRESENCE | ShouldExist={shouldExist} | MatchCount={count} | " +
                 $"Record: {record.University} | {record.Country} | {record.Title} | " +
                 $"{record.Degree} | {record.GraduationYear.ToString()}");
            Info($"Table snapshot: [{details}]");

            if (shouldExist)
            {
                Assert.That(count, Is.GreaterThan(0),
                    $"Missing education row: \"{record.University}\" | \"{record.Country}\" | " +
                    $"\"{record.Title}\" | \"{record.Degree}\" | \"{record.GraduationYear.ToString()}\". " +
                    $"Table: [{details}]");
            }
            else
            {
                Assert.That(count, Is.EqualTo(0),
                    $"Unexpected education row found {count} time(s): \"{record.University}\" | " +
                    $"\"{record.Country}\" | \"{record.Title}\" | \"{record.Degree}\" | " +
                    $"\"{record.GraduationYear.ToString()}\". Table: [{details}]");
            }
        }
        private void AssertEducationCount(EducationRecord record, int expectedCount)
        {
            var count = EducationPage.CountEducationRows(record);
            var details = EducationPage.GetEducationDetails();

            Assert.That(count, Is.EqualTo(expectedCount),
                $"Expected {expectedCount} matching row(s) for: " +
                $"\"{record.University}\" | \"{record.Country}\" | \"{record.Title}\" | " +
                $"\"{record.Degree}\" | \"{record.GraduationYear.ToString()}\". Actual: {count}. " +
                $"Table: [{details}]");
        }


    }
}
