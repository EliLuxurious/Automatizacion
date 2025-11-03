using System;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;          // <- SelectElement
using Reqnroll;
using SeleniumExtras.WaitHelpers;

namespace Automation.StepDefinitions
{
    [Binding]
    public class NuevoIngresoStepDefinitions
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;

        // ===== Hooks =====
        [BeforeScenario]
        public void BeforeScenario()
        {
            _driver = new ChromeDriver();
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
        }

        [AfterScenario]
        public void AfterScenario()
        {
            _driver.Quit();
            _driver.Dispose();
        }

        // ===== Steps =====

        [Given("el usuario ingresa al ambiente {string}")]
        public void GivenElUsuarioIngresaAlAmbiente(string url)
        {
            _driver.Navigate().GoToUrl(url);
            _driver.Manage().Window.Maximize();
        }

        [When("el usuario ingresa sesion con usuario {string} y contraseña {string}")]
        public void WhenElUsuarioIngresaSesionConUsuarioYContrasena(string usuario, string clave)
        {
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Email"))).SendKeys(usuario);
            _driver.FindElement(By.Id("Password")).SendKeys(clave);
            _driver.FindElement(By.XPath("//button[normalize-space()='Iniciar']")).Click();

            // Aceptar diálogo y validar login
            _wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[normalize-space()='Aceptar']"))).Click();
            Assert.IsTrue(_wait.Until(ExpectedConditions.ElementIsVisible(By.Id("ImagenLogo"))).Displayed);
        }

        [When("acceder al modulo {string}")]
        public void WhenAccederAlModulo(string modulo)
        {
            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.XPath($"//span[normalize-space()='{modulo}']"))).Click();
        }

        [When("acceder al submodulo {string}")]
        public void WhenAccederAlSubmodulo(string submodulo)
        {
            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.XPath($"//a[normalize-space()='{submodulo}']"))).Click();
        }

        [When("el usuario hace clic en el botón {string}")]
        public void WhenElUsuarioHaceClicEnElBoton(string textoBoton)
        {
            _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.XPath($"//button[contains(normalize-space(.), '{textoBoton}')]"))).Click();
        }

        // ÚNICO step genérico para completar campos
        [When("completa el campo {string} con {string}")]
        public void WhenCompletaElCampoCon(string campo, string valor)
        {
            By locator;

            switch (campo.Trim().ToUpperInvariant())
            {
                case "AUTORIZADO POR":
                    locator = By.XPath("//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='AUTORIZADO POR']/following::input[1]");
                    break;

                case "PAGADOR":
                    locator = By.XPath("//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='PAGADOR']/following::input[1]");
                    break;

                case "IMPORTE":
                    locator = By.Id("importe");
                    break;

                case "OBSERVACIÓN":
                case "OBSERVACION":
                    locator = By.Id("observacion");
                    break;

                default:
                    throw new ArgumentException($"Campo no soportado: {campo}");
            }

            var el = _wait.Until(ExpectedConditions.ElementIsVisible(locator));
            el.Clear();
            el.SendKeys(valor);
        }

        [When("selecciona {string} como tipo de persona")]
        public void WhenSeleccionaComoTipoDePersona(string tipo)
        {
            // Click al label (Empleado/Cliente/Proveedor) dentro del modal
            var label = _wait.Until(ExpectedConditions.ElementToBeClickable(
                By.XPath($"//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='{tipo}']")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'})", label);
            label.Click();

            // Si el label no marca, clic al input hermano
            var radio = _driver.FindElements(By.XPath(
                $"//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='{tipo}']/preceding-sibling::input[@type='radio'] | " +
                $"//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='{tipo}']/following-sibling::input[@type='radio']"))
                .FirstOrDefault();

            if (radio != null && !radio.Selected)
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", radio);
                if (!radio.Selected)
                    throw new WebDriverException($"No se pudo seleccionar el tipo '{tipo}'.");
            }
        }

        [When("selecciona el tipo de documento {string}")]
        public void WhenSeleccionaElTipoDeDocumento(string documentoVisibleText)
        {
            // Espera modal para acotar
            _wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='modal-registro-ingreso-egreso-varios']")));

            var ddl = new SelectElement(_wait.Until(ExpectedConditions.ElementExists(
                By.XPath("//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='DOCUMENTO']/following::select[1]"))));
            ddl.SelectByText(documentoVisibleText);
        }

        [When("completa el importe con {string}")]
        public void WhenCompletaElImporteCon(string monto)
        {
            var importe = _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("importe")));
            importe.Clear();
            importe.SendKeys(monto);
        }

        [When("escribe la observación {string}")]
        public void WhenEscribeLaObservacion(string obs)
        {
            var observacion = _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("observacion")));
            observacion.Clear();
            observacion.SendKeys(obs);
        }

        [When("presiona el botón {string}")]
        public void WhenPresionaElBoton(string textoBoton)
        {
            // Si es GUARDAR, esperar que esté habilitado
            if (textoBoton.Trim().Equals("GUARDAR", StringComparison.OrdinalIgnoreCase))
            {
                var btn = EsperarBotonGuardarHabilitado();
                btn.Click();
            }
            else
            {
                _wait.Until(ExpectedConditions.ElementToBeClickable(
                    By.XPath($"//button[normalize-space()='{textoBoton}']"))).Click();
            }
        }

        [Then("el ingreso se registra correctamente")]
        public void ThenElIngresoSeRegistraCorrectamente()
        {
            var mensajeOk = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.XPath("//*[contains(.,'Ingreso registrado correctamente') or contains(.,'guardado')]")));
            Assert.IsTrue(mensajeOk.Displayed, "No se mostró mensaje de confirmación de guardado.");
        }

        [Then("se muestra el mensaje de error '(.*)'")]
        public void ThenSeMuestraElMensajeDeError(string mensaje)
        {
            var errorMsg = _wait.Until(ExpectedConditions.ElementIsVisible(
                By.XPath($"//*[contains(text(),'{mensaje}')]")));
            Assert.IsTrue(errorMsg.Displayed, $"No se encontró el mensaje: {mensaje}");
        }


        // ===== Helpers privados =====
        private IWebElement EsperarBotonGuardarHabilitado()
        {
            return _wait.Until(driver =>
            {
                var btn = driver.FindElement(By.XPath(
                    "//*[@id='modal-registro-ingreso-egreso-varios']//button[@title='GUARDAR' or normalize-space()='GUARDAR' or @ng-click='registrarIngresoEgresoVarios()']"));
                var disabledAttr = btn.GetAttribute("disabled");
                return (btn.Enabled && string.IsNullOrEmpty(disabledAttr)) ? btn : null;
            });
        }
    }
}
