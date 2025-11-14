using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Linq;
using NUnit.Framework;

namespace RegistroIngreso
{
    [TestFixture]
    public class RegistroIngreso
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;

        [SetUp]
        public void Setup()
        {
            _driver = new ChromeDriver();
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
        }

        //[TearDown]
        //public void TearDown()
        //{
        //    _driver.Quit();
        //    _driver.Dispose();
        //}

        [Test]
        public void RegistrarIngresoVarios()
        {
            // 1) Navegar al sistema
            _driver.Navigate().GoToUrl("https://taller2025-qa.sigesonline.com/");
            _driver.Manage().Window.Maximize();

            // 2) Login
            var usernameField = _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Email")));
            var passwordField = _driver.FindElement(By.Id("Password"));
            var loginButton = _driver.FindElement(By.XPath("//button[normalize-space()='Iniciar']"));

            usernameField.SendKeys("admin@plazafer.com");
            passwordField.SendKeys("calidad");
            loginButton.Click();

            // 3) Aceptar ventana de confirmación
            _wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[normalize-space()='Aceptar']"))).Click();

            // 4) Validar login exitoso
            Assert.IsTrue(_wait.Until(ExpectedConditions.ElementIsVisible(By.Id("ImagenLogo"))).Displayed);
            //Ingresar al módulo

            var saleButton = _driver.FindElement(By.XPath("//a[@class='menu-lista-cabecera']/span[text()='Tesorería y Finanzas']"));
            saleButton.Click();
            Thread.Sleep(4000);

            // 6) Submódulo Ingresos/Egresos
            var newSaleButton = _driver.FindElement(By.XPath("//a[normalize-space()='Ingresos/Egresos']"));
            newSaleButton.Click();

            // 7) Botón INGRESO
            _wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[contains(., 'INGRESO')]"))).Click();

            // 8) Esperar el modal y completar formulario
            var modal = EsperarModal();

            // Autorizado por (label -> input siguiente)
            var autorizadoInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.XPath("//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='AUTORIZADO POR']/following::input[1]")));
            autorizadoInput.Clear();
            autorizadoInput.SendKeys("11111110");

            // Tipo de persona: Empleado / Cliente / Proveedor
            SeleccionarTipoPagador("Empleado"); // Cambia por "Cliente" o "Proveedor" cuando lo necesites

            // Pagador
            var pagadorInput = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.XPath("//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='PAGADOR']/following::input[1]")));
            pagadorInput.Clear();
            pagadorInput.SendKeys("11111110");

            // Documento (NOTA DE INGRESO)
            var ddlDocumento = new SelectElement(_wait.Until(ExpectedConditions.ElementExists(
                By.XPath("//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='DOCUMENTO']/following::select[1]"))));
            ddlDocumento.SelectByText("NOTA DE INGRESO");

            // Importe
            var imp = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.XPath("//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='IMPORTE']/following::input[1]")));
            imp.Click(); imp.SendKeys(Keys.Control + "a"); imp.SendKeys(Keys.Delete);
            imp.SendKeys("0"); imp.SendKeys(Keys.Tab);


            // Observación (for="observacion")
            var obs = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.XPath("//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='OBSERVACIÓN' or normalize-space()='OBSERVACION']/following::*[self::textarea or self::input][1]")
            ));
            obs.Clear();
            obs.SendKeys("Pago de servicio");



            // 9) Guardar (esperar que esté habilitado)
            var btnGuardar = EsperarBotonGuardarHabilitado();
            btnGuardar.Click();
        
        }

        // ===== Helpers =====

        private IWebElement EsperarModal()
        {
            return _wait.Until(ExpectedConditions.ElementIsVisible(
                By.XPath("//*[@id='modal-registro-ingreso-egreso-varios']")));
        }

        private void SeleccionarTipoPagador(string textoLabel)
        {
            // Click al label por texto dentro del modal
            var label = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.XPath($"//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='{textoLabel}']")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'})", label);
            label.Click();

            // Si el label no cambia el estado, forzamos click al input radio hermano
            var radio = _driver.FindElements(By.XPath(
                $"//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='{textoLabel}']/preceding-sibling::input[@type='radio'] | " +
                $"//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='{textoLabel}']/following-sibling::input[@type='radio']"))
                .FirstOrDefault();

            if (radio != null && !radio.Selected)
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", radio);
                if (!radio.Selected)
                    throw new WebDriverException($"No se pudo seleccionar el tipo '{textoLabel}'.");
            }
        }

        private IWebElement EsperarBotonGuardarHabilitado()
        {
            return _wait.Until(driver =>
            {
                var btn = driver.FindElement(By.XPath(
                    "//*[@id='modal-registro-ingreso-egreso-varios']//button[@title='GUARDAR' or normalize-space()='GUARDAR' or @ng-click='registrarIngresoEgresoVarios()']"));

                // enabled y sin atributo disabled
                var disabledAttr = btn.GetAttribute("disabled");
                if (btn.Enabled && string.IsNullOrEmpty(disabledAttr))
                    return btn;

                return null;
            });
        }
        [TearDown]
        public void TearDown()
        {
            try
            {
                try
                {
                    if (_driver != null && _driver.Manage() != null && _driver.Manage().Timeouts() != null)
                        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(0);
                }
                catch { }

                try
                {
                    if (_driver != null)
                    {
                        try { _driver.SwitchTo().Alert().Dismiss(); } catch { }
                        var handles = _driver.WindowHandles;
                        foreach (var h in handles)
                        {
                            try { _driver.SwitchTo().Window(h); _driver.Close(); } catch { }
                        }
                    }
                }
                catch { }
            }
            finally
            {
                try { if (_driver != null) _driver.Quit(); } catch { }
                try { if (_driver != null) _driver.Dispose(); } catch { }
                _driver = null;
            }
        }

    }
}
