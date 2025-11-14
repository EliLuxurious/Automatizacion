// MenuPage
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace Automation.Pages
{
    public class MenuPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        public MenuPage(IWebDriver d, WebDriverWait w) { _driver = d; _wait = w; }

        public void OpenModule(string modulo)
        {
            string target = modulo.Trim().ToLowerInvariant();

            var moduloSpan = By.XPath(
                "//*[(self::span or self::a)]" +
                "[translate(normalize-space(.), " +
                "'ÁÉÍÓÚÄËÏÖÜáéíóúäëïöüABCDEFGHIJKLMNOPQRSTUVWXYZ', " +
                "'AEIOUAEIOUAEIOUAEIOUabcdefghijklmnopqrstuvwxyz')=" +
                $"translate('{target}','ÁÉÍÓÚÄËÏÖÜáéíóúäëïöüABCDEFGHIJKLMNOPQRSTUVWXYZ','AEIOUAEIOUAEIOUAEIOUabcdefghijklmnopqrstuvwxyz')]" +
                "/ancestor-or-self::a[1]"
            );

            var moduloContains = By.XPath(
                "//a[.//span or text()]" +
                "[contains(translate(normalize-space(.), " +
                "'ÁÉÍÓÚÄËÏÖÜáéíóúäëïöüABCDEFGHIJKLMNOPQRSTUVWXYZ', " +
                "'AEIOUAEIOUAEIOUAEIOUabcdefghijklmnopqrstuvwxyz'), " +
                $"translate('{target}','ÁÉÍÓÚÄËÏÖÜáéíóúäëïöüABCDEFGHIJKLMNOPQRSTUVWXYZ','AEIOUAEIOUAEIOUAEIOUabcdefghijklmnopqrstuvwxyz'))]"
            );

            _wait.Until(ExpectedConditions.ElementExists(
                By.XPath("//nav | //aside | //ul[contains(@class,'menu') or contains(@class,'sidebar')]")));

            IWebElement el = null;
            try { el = _wait.Until(ExpectedConditions.ElementToBeClickable(moduloSpan)); }
            catch { el = _wait.Until(ExpectedConditions.ElementToBeClickable(moduloContains)); }

            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'})", el);
            try { el.Click(); } catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", el); }
        }

        public void OpenSubmodule(string sub)
        {
            // 1) Por href (según tu HTML real)
            var byHref = By.CssSelector("a[href*='/Finanza/CobrosYPagos']");

            // 2) Fallback por texto normalizado
            string target = sub.Trim().ToLowerInvariant();
            var byText = By.XPath(
                "//a[contains(translate(normalize-space(.), " +
                "'ÁÉÍÓÚÄËÏÖÜáéíóúäëïöüABCDEFGHIJKLMNOPQRSTUVWXYZ', " +
                "'AEIOUAEIOUAEIOUAEIOUabcdefghijklmnopqrstuvwxyz'), " +
                $"translate('{target}','ÁÉÍÓÚÄËÏÖÜáéíóúäëïöüABCDEFGHIJKLMNOPQRSTUVWXYZ','AEIOUAEIOUAEIOUAEIOUabcdefghijklmnopqrstuvwxyz'))]"
            );

            IWebElement el = null;
            try { el = _wait.Until(ExpectedConditions.ElementToBeClickable(byHref)); }
            catch { el = _wait.Until(ExpectedConditions.ElementToBeClickable(byText)); }

            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'})", el);
            try { el.Click(); } catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", el); }
        }

    }
}
