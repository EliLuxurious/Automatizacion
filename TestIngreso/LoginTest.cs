using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EjemploLogin
{
    public class LoginTest
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;

        [SetUp]
        public void Setup()
        {
            _driver = new ChromeDriver();
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        [Test]
        public void TestExampleTest()
        {
            // Navegar a la URL
            _driver.Navigate().GoToUrl("https://taller2025-qa.sigesonline.com/");
            Thread.Sleep(4000);

            // Encontrar los elementos del login
            var usernameField = _driver.FindElement(By.XPath("//input[@id='Email']"));
            var passwordField = _driver.FindElement(By.XPath("//input[@id='Password']"));
            var loginButton = _driver.FindElement(By.XPath("//button[normalize-space()='Iniciar']"));

            usernameField.SendKeys("admin@plazafer.com");
            Thread.Sleep(2000);
            passwordField.SendKeys("calidad");
            Thread.Sleep(2000);

            loginButton.Click();
            Thread.Sleep(4000);

            // Hacer clic en aceptar
            var acepptButton = _driver.FindElement(By.XPath("//button[normalize-space()='Aceptar']"));
            acepptButton.Click();

            // Comprobar que el login fue exitoso
            var succesElement = _wait.Until(drv => drv.FindElement(By.XPath("//img[@id='ImagenLogo']")));
            Assert.IsNotNull(succesElement, "No se encontró el elemento de éxito después del login.");
        }

        [TearDown]
        public void TearDown()
        {
            // Cerrar el navegador después de cada prueba
            _driver.Quit();
            _driver.Dispose(); // Libera memoria y recursos no administrados
        }

    }
}
