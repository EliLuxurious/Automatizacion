using System;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Automation.Pages.Helpers
{
    public class Utilities
    {
        private readonly IWebDriver _driver;
        private readonly OpenQA.Selenium.Support.UI.WebDriverWait _wait;
        private readonly IJavaScriptExecutor _js;

        public Utilities(IWebDriver driver, OpenQA.Selenium.Support.UI.WebDriverWait wait)
        {
            _driver = driver; _wait = wait; _js = (IJavaScriptExecutor)driver;
        }

        // ... (resto igual)

        public void Screenshot(string namePrefix = "shot_")
        {
            try
            {
                var ss = ((ITakesScreenshot)_driver).GetScreenshot();
                var dir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "screenshots");
                Directory.CreateDirectory(dir);
                var path = Path.Combine(dir, $"{namePrefix}{DateTime.Now:yyyyMMdd_HHmmssfff}.png");
                ss.SaveAsFile(path); // <- SIN ScreenshotImageFormat
                TestContext.AddTestAttachment(path, "Evidencia");
            }
            catch { }
        }
    }
}
