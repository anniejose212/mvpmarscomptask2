# Mars Competition Test Automation Report

## Configuration Overview


## Automation Framework Details

### Technology Stack
- NUnit Framework with Selenium WebDriver in C#
- Page Object Model (POM) design pattern for UI abstraction
- JSON reader utility to load test data from JSON files
- Extent Reports for rich HTML reporting with screenshots on failures

### Test Data Management
- Test data is stored as JSON files
- Data is deserialized into strongly-typed objects used by tests

### Reporting Features
- Extent Reports are generated automatically for each test run and saved at the configured report path
- Screenshots of failed tests are automatically captured and attached to the report for easier debugging
- (Recommendation) Screenshots for passing tests can be enabled manually in specific test cases for demonstration purposes

```csharp 
var screenshotPath = SaveScreenshot("TestName");
if (!string.IsNullOrEmpty(screenshotPath)) Test.AddScreenCaptureFromPath(screenshotPath); 
Test.Log(Status.Info, "Screenshot attached after successful add for demo purposes."); 

```

            

### Test Lifecycle Hooks
- Common driver setup and teardown are centralized in the Base class
- Before and after virtual hooks clean up certification and education data to ensure clean state
- Browser driver is initialized with settings from the JSON config

### Code Organization
- Tests for Certification and Education features are separated into dedicated test classes
- Page Objects encapsulate all UI interactions with the certification and education pages
- Helper classes support toast message handling and other reusable UI elements
- Assertions use NUnit's assertion framework effectively with clear failure messages

---

## How to Use

1. Update the `testsettings.json` configuration file for your environment.
2. Build and run tests using NUnit test runner.
3. After test execution, open the Extent Report located at `Reports/MarsTestReport.html`.
4. Review detailed test logs, screenshots (on failure), and pass/fail status.
5. Optional explicit screenshot code for passing test is available in one test case for demonstration:AddCertification_AddsRowAndShowsSuccessToast


## Robustness and Security Validation Tests

These automated tests verify that the Education and Certification modules securely handle invalid, empty, or malicious data.  
They ensure that invalid entries are rejected, harmful scripts are neutralized, and users receive clear feedback through alerts or toast messages.

### Purpose
- Ensure the system rejects meaningless or malformed inputs (such as whitespace or random symbols).  
- Block cross-site scripting (XSS) attempts so that no HTML or JavaScript executes.  
- Prevent SQL or injection-like inputs from being processed as real database queries.  
- Confirm that appropriate error toasts are displayed and that no invalid records are saved.

### Standard Test Procedure
1. Load test data from JSON files (`Education...json` / `Certification...json`).  
2. Open the corresponding tab and fill out the form with that data.  
3. Click **Add** to submit.  
4. Capture all displayed alerts or toasts (`PrintAllToasts`).  
5. Validate that:  
   - No success toast appears.  
   - An error toast or alert may appear.  
   - Invalid data is not stored in the table or database.

### Test Data Types
- **Robustness Tests:** Inputs with whitespace, special characters, or placeholder strings.  
- **Security Tests:**  
  - *XSS Attacks:* `<script>` tags, inline `onerror=` payloads, or embedded HTML snippets.  
  - *SQL Injection:* Inputs like `' OR '1'='1`, `DROP TABLE`, or other query-like strings.

### Observed Results
Most of the tests currently fail because the system accepts unsafe input without proper validation or sanitization.  
XSS and SQL-style payloads are being stored rather than rejected, revealing weaknesses in input filtering and validation.  
Error toasts are missing or appear inconsistently, allowing potentially harmful data to persist.

### Expected Results
All invalid, XSS, or injection-style entries should be securely blocked, trigger clear error messages, and result in no data changes.


# 🧾 Reports and Screenshots Setup

This project automatically generates both **Extent HTML Reports** and **Screenshots** for each test run.  
The setup is environment-independent — it works the same locally and on GitHub Actions.

---

### Browser Settings
- **Type:** Chrome
- **Headless Mode:** Enabled
- **Timeout:** 30 seconds


### Environment
- **Base URL:** http://localhost:5003

### Login Credentials
- **Username:** testingcomp@gmail.com
- **Password:** 123456

---

## 📊 Extent Report

**Configured in:** `testsettings.json`
```json
"Report": {
  "Path": "Reports/MarsTestReport.html",
  "Title": "Mars Competition Test Automation Report"
}
```

**How the path is resolved (in `Base.cs`):**
```csharp
var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
var reportPath = Path.Combine(root, Settings.Report.Path);
```

**📍 Final locations:**
| Environment | Path |
|--------------|------|
| **Local (Windows)** | `C:\Users\<user>\source\repos\MarsNunitJson\Reports\MarsTestReport.html` |
| **GitHub Actions (Linux)** | `/home/runner/work/MarsNunitJson/MarsNunitJson/Reports/MarsTestReport.html` |

**ℹ️ Notes**
- The `ExtentSparkReporter` writes to the path defined above.  
- The report is finalized and flushed at the end of the run inside:
  ```csharp
  [OneTimeTearDown]
  public void GlobalTeardown()
  {
      Extent?.Flush();
      Console.WriteLine($"[INFO] Report flushed: {Settings?.Report?.Path}");
  }
  ```

---

## 📸 Screenshots

**Generated automatically** for failed tests by `SaveScreenshot()` in `Base.cs`.

**Screenshot path logic:**
```csharp
var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
var dir = Path.Combine(root, "Screenshots");
```

**📍 Final locations:**
| Environment | Path |
|--------------|------|
| **Local (Windows)** | `C:\Users\<user>\source\repos\MarsNunitJson\Screenshots\` |
| **GitHub Actions (Linux)** | `/home/runner/work/MarsNunitJson/MarsNunitJson/Screenshots/` |

**📄 Example file name**
```
AddCertification_AddsRowAndShowsSuccessToast_20251109_115314.png
```

**🧠 Behavior**
- On any test failure, a screenshot is taken and attached to the HTML report.  
- Console output example:  
  ```
  [SCREENSHOT] C:\Users\<user>\source\repos\MarsNunitJson\Screenshots\AddCertification_AddsRowAndShowsSuccessToast_20251109_115314.png
  ```

---

## ⚙️ GitHub Actions Integration

To keep reports and screenshots as downloadable artifacts, add these steps to your workflow:

```yaml
- name: Upload Extent report
  uses: actions/upload-artifact@v4
  with:
    name: extent-report
    path: Reports/

- name: Upload screenshots
  uses: actions/upload-artifact@v4
  with:
    name: screenshots
    path: Screenshots/
```

---

## ✅ Summary

| Artifact | Config Source | Example Path (Local) | Example Path (GitHub) | Generated By |
|-----------|----------------|----------------------|------------------------|---------------|
| **Extent Report** | `testsettings.json → Report.Path` | `...\Reports\MarsTestReport.html` | `/Reports/MarsTestReport.html` | `ExtentReports` |
| **Screenshots** | Hardcoded in `Base.SaveScreenshot()` | `...\Screenshots\*.png` | `/Screenshots/*.png` | WebDriver on failure |

---

## 🧩 Troubleshooting

**🟥 Report not visible?**  
- Check `bin/Debug/net8.0/Reports/` — by default, NUnit runs from the `bin` folder.  
- If you’ve already updated `Base.cs` to anchor reports to project root, confirm this line is printed in your test console:  
  ```
  [INFO] Extent report path: C:\Users\<user>\source\repos\MarsNunitJson\Reports\MarsTestReport.html
  ```
- If not printed, the `Base.GlobalSetup()` might not have executed or `testsettings.json` path is invalid.

**🟨 Screenshots not saved?**  
- Ensure the test actually failed. Screenshots are only taken inside the `[TearDown]` block for failed tests.  
- Check permissions for the `Screenshots` directory.  
- Verify that the ChromeDriver window is not being killed before `SaveScreenshot()` executes.

**🟩 GitHub Actions artifacts missing?**  
- Make sure the `upload-artifact` steps point to `Reports/` and `Screenshots/` (relative to project root).  
- Artifacts only appear after the run completes successfully; failed jobs may skip upload unless `if: always()` is set:
  ```yaml
  - name: Upload Reports and Screenshots
    if: always()
    uses: actions/upload-artifact@v4
    with:
      name: test-artifacts
      path: |
        Reports/
        Screenshots/
  ```
