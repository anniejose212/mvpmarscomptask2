using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;


public class ToastHelper
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    private static readonly By errorToast =
       By.CssSelector("div.ns-box.ns-type-error .ns-box-inner");

    private static readonly By successToast =
        By.CssSelector("div.ns-box.ns-type-success .ns-box-inner");


    private readonly By closeButton =
           By.XPath("//a[@class='ns-close']");


    public ToastHelper(IWebDriver driver, int timeoutSeconds = 10)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
    }

    public string GetSuccessToastText()
    {
        
        var toast = _wait.Until(driver => driver.FindElement(successToast));
        return toast.Text.Trim();
    }

    
    public string GetErrorToastText()
    {
        
        var toast = _wait.Until(driver => driver.FindElement(errorToast));
        return toast.Text.Trim();
    }

    public void CloseToast()
    {
        var buttons = _driver.FindElements(closeButton);
        foreach (var btn in buttons)
        {
            if (btn.Displayed && btn.Enabled)
            {
                btn.Click();
            }
        }
    }

    public void PrintAllToasts()
    {
        var toasts = _driver.FindElements(By.CssSelector("div.ns-box-inner"));
        Console.WriteLine($"Found {toasts.Count} toast(s).");

        foreach (var toast in toasts)
        {
            var text = toast.Text.Trim();
            Console.WriteLine($"Toast text: {text}");
        }
    }
}
