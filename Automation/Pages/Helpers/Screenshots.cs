using System;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Automation.Pages.Helpers
{
    public static class Screenshots
    {
        public static void Take(IWebDriver driver, string prefix = "shot_")
        {
            try
            {
                var ss = ((ITakesScreenshot)driver).GetScreenshot();
                var dir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "screenshots");
                Directory.CreateDirectory(dir);
                var path = Path.Combine(dir, $"{prefix}{DateTime.Now:yyyyMMdd_HHmmssfff}.png");
                ss.SaveAsFile(path); // <- SIN ScreenshotImageFormat
                TestContext.AddTestAttachment(path, "Evidencia");
            }
            catch { /* no romper */ }
        }
    }
}
