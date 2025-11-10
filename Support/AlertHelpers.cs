// AlertHelpers.cs
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace mars_nunit_json.Support

{
    public static class AlertHelpers
    {
        
        public static IAlert TryGetAlert(this IWebDriver driver, int seconds = 2)
        {
            if (driver == null) return null;

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));
            try
            {
                return wait.Until(d =>
                {
                    try { return d.SwitchTo().Alert(); }
                    catch (NoAlertPresentException) { return null; }
                });
            }
            catch (WebDriverTimeoutException)
            {
                return null;
            }
        }

     
        public static bool TryDismissAnyAlert(this IWebDriver driver, int seconds = 0)
        {
            if (driver == null) return false;

            
            var alert = seconds > 0 ? driver.TryGetAlert(seconds) : driver.TryGetAlert(0);
            if (alert == null) return false;

            try
            {
                var text = alert.Text; 
                Console.WriteLine($"[TEARDOWN] Dismissing stray alert: '{text}'");
                alert.Accept(); 
                return true;
            }
            catch
            {
                return false; 
            }
        }
    }
}
