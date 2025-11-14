// LoginPage
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace Automation.Pages
{
    public class LoginPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        public LoginPage(IWebDriver d, WebDriverWait w) { _driver = d; _wait = w; }

        public void GoTo(string url)
        {
            _driver.Navigate().GoToUrl(url);
            _driver.Manage().Window.Maximize();
        }

        public void SignIn(string user, string pass)
        {
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Email"))).SendKeys(user);
            _driver.FindElement(By.Id("Password")).SendKeys(pass);
            _driver.FindElement(By.XPath("//button[normalize-space()='Iniciar']")).Click();

            // Aceptar modal de bienvenida/confirmación si aparece
            try
            {
                _wait.Until(ExpectedConditions.ElementToBeClickable(
                    By.XPath("//button[normalize-space()='Aceptar']"))).Click();
            }
            catch { /* puede no salir */ }

            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("ImagenLogo")));
        }
    }
}
