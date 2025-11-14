//using Automation.Pages.Helpers;
//using NUnit.Framework;
//using OpenQA.Selenium;
//using OpenQA.Selenium.Chrome;
//using OpenQA.Selenium.Support.UI;
//using Automation.Pages.Helpers;            // Screenshots.Take(...)
//using System;                                   // TimeSpan, Environment

//namespace Support
//{
//    public abstract class BaseTest
//    {
//        protected IWebDriver Driver;
//        protected WebDriverWait Wait;

//        [SetUp]
//        public virtual void SetUp()
//        {
//            var options = new ChromeOptions();

//            // Opcional: habilita modo headless si defines HEADLESS=true
//            var headless = Environment.GetEnvironmentVariable("HEADLESS");
//            if (!string.IsNullOrEmpty(headless) &&
//                headless.Equals("true", StringComparison.OrdinalIgnoreCase))
//            {
//                options.AddArgument("--headless=new");
//            }

//            // Opcional: estabilidad en CI
//            options.AddArgument("--disable-gpu");
//            options.AddArgument("--no-sandbox");
//            options.AddArgument("--disable-dev-shm-usage");

//            Driver = new ChromeDriver(options);
//            Driver.Manage().Window.Size = new System.Drawing.Size(1440, 900);

//            Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(Config.TimeoutSeconds));
//        }

//        [TearDown]
//        public virtual void TearDown()
//        {
//            try
//            {
//                var status = TestContext.CurrentContext.Result.Outcome.Status.ToString();
//                Screenshots.Take(Driver, status + "_");
//            }
//            catch { /* no romper teardown por la captura */ }

//            try { Driver?.Quit(); } catch { }
//            try { Driver?.Dispose(); } catch { }
//        }
//    }
//}
