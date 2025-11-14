using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Reqnroll;
using Reqnroll.BoDi;
using SeleniumExtras.WaitHelpers;
using System;
using System.IO;

namespace Automation.Hooks
{
    [Binding]
    [Parallelizable(ParallelScope.None)] // evita instancias paralelas peleándose por el mismo browser
    public class Hooks
    {
        private readonly IObjectContainer _container;
        private IWebDriver _driver;
        private WebDriverWait _wait;
        private readonly ScenarioContext _scenario;

        public Hooks(IObjectContainer container, ScenarioContext scenario)
        {
            _container = container;
            _scenario = scenario;
        }

        [BeforeScenario(Order = 0)]
        public void StartBrowser()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--no-sandbox");
            // options.AddArgument("--headless=new"); // opcional: headless en CI

            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            _driver = new ChromeDriver(service, options);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(0); // solo esperas explícitas

            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));

            // registra para DI (tus StepDefinitions reciben estos objetos)
            _container.RegisterInstanceAs(_driver);
            _container.RegisterInstanceAs(_wait);
        }

        [AfterScenario(Order = 100)]
        public void StopBrowser()
        {
            try
            {
                // si el modal sigue abierto, intenta cerrarlo para evitar overlays entre escenarios
                try
                {
                    var modal = _driver?.FindElements(By.Id("modal-registro-ingreso-egreso-varios"));
                    if (modal != null && modal.Count > 0)
                    {
                        // botón CANCELAR/X si existe
                        var cancelar = _driver.FindElements(By.XPath(
                            "//*[@id='modal-registro-ingreso-egreso-varios']//button[@title='CANCELAR' or normalize-space()='CANCELAR' or @data-dismiss='modal']"));
                        if (cancelar.Count > 0)
                        {
                            try { cancelar[0].Click(); }
                            catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", cancelar[0]); }
                            _wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.Id("modal-registro-ingreso-egreso-varios")));
                        }
                    }
                }
                catch { /* si no se puede, seguimos cerrando igual */ }

                // screenshot si falló
                if (_scenario.TestError != null)
                {
                    try
                    {
                        var file = Path.Combine(TestContext.CurrentContext.WorkDirectory,
                                                $"fail_{DateTime.Now:yyyyMMdd_HHmmssfff}.png");
                        var shot = ((ITakesScreenshot)_driver).GetScreenshot();
                        shot.SaveAsFile(file);
                    }
                    catch { }
                }
            }
            finally
            {
                // cierre fuerte del navegador SIEMPRE
                try { if (_driver != null) { _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(0); } } catch { }
                try { _driver?.Quit(); } catch { }
                try { _driver?.Dispose(); } catch { }
                _driver = null;
            }
        }
    }
}
