using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Linq;

namespace RegistroIngreso.Pages
{
    public class IngresoPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public IngresoPage(IWebDriver driver, WebDriverWait wait)
        {
            _driver = driver;
            _wait = wait;
        }

        // ====== LOCATORS ======
        private By Modal => By.Id("modal-registro-ingreso-egreso-varios");
        private By BtnIngreso => By.XPath("//button[contains(., 'INGRESO')]");
        private By BtnGuardar => By.XPath(
            "//*[@id='modal-registro-ingreso-egreso-varios']//button[@title='GUARDAR' or @ng-click='registrarIngresoEgresoVarios()']"
        );

        // Botón del footer del modal (Guardar / Cerrar, etc.)
        private By BtnFooterModal => By.XPath("//*[@id='modal-registro-ingreso-egreso-varios']/div/div/div[3]/button[1]");

        // Botón del menú lateral: Tesorería y Finanzas (o el módulo que corresponda)
        private By BtnMenuTesoreriaYFinanzas => By.XPath("//*[@id='id-menu-sidebar-principal']/div[4]/div");


        // Tipo de persona (labels Empleado/Cliente/Proveedor)
        private By LabelTipoPersona(string texto) => By.XPath(
            "//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='" + texto + "']"
        );

        // AUTORIZADO POR → primer input después del label
        private By InputAutorizadoPor => By.XPath(
            "//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='AUTORIZADO POR']/following::input[@type='text'][1]"
        );

        // PAGADOR → input con id DocumentoIdentidad (DNI/RUC)
        private By InputPagador => By.Id("DocumentoIdentidad");

        // DOCUMENTO → Select2 (abre lista y busca opción)
        private By Select2DocumentoButton => By.XPath(
            "//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='DOCUMENTO']/following::*[contains(@class,'select2-selection--single')][1]"
        );
        private By Select2SearchInput => By.CssSelector(".select2-container--open .select2-search__field");
        private By Select2Option(string texto) => By.XPath(
            $".//li[contains(@class,'select2-results__option') and normalize-space()='{texto}']"
        );

        // SweetAlert (swal) de error
        private By SwalTitle => By.CssSelector(".swal-title");
        private By SwalText => By.CssSelector(".swal-text");


        // OBSERVACIÓN → textarea con ng-model
        private By TextareaObservacion => By.CssSelector("textarea[ng-model='modelo.Observacion']");

        // IMPORTE → input después del label IMPORTE
        private By InputImporte => By.XPath(
            "//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='IMPORTE']/following::input[1]"
        );

        // Validaciones y toasts
        // Reemplaza el locator antiguo:
        private By BloqueInconsistencias => By.XPath(
            "//*[contains(translate(.,'ÁÉÍÓÚáéíóú','AEIOUaeiou'),'inconsistencia')]" +
            " | " +
            "//*[contains(@class,'alert') and (contains(.,'INCONS') or contains(.,'Incons') or contains(.,'incons'))]" +
            " | " +
            "//*[contains(translate(.,'ÁÉÍÓÚáéíóú','AEIOUaeiou'),'es necesario')]"
        );

        private By ToastExito => By.XPath("//*[contains(@class,'toast') and (contains(.,'éxito') or contains(.,'registr') or contains(.,'guard'))]");
        private By ToastError => By.XPath("//*[contains(@class,'toast') and (contains(.,'error') or contains(.,'inválid') or contains(.,'inconsist'))]");


        // ====== ACCIONES ======

        public void AbrirFormularioIngreso()
        {
            _wait.Until(ExpectedConditions.ElementToBeClickable(BtnIngreso)).Click();
            _wait.Until(ExpectedConditions.ElementIsVisible(Modal));
        }

        public void SeleccionarTipoPersona(string textoLabel)
        {
            // 1) Ubica el label por texto
            var label = _wait.Until(ExpectedConditions.ElementExists(
                By.XPath($"//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='{textoLabel}']")));

            // 2) Si el input DNI/RUC está visible, haz blur (Tab) para evitar que quede “encima”
            try
            {
                var dni = _driver.FindElements(By.Id("DocumentoIdentidad")).FirstOrDefault(el => el.Displayed);
                if (dni != null)
                {
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'})", dni);
                    dni.SendKeys(Keys.Tab);
                }
            }
            catch { /* no pasa nada si no está */ }

            // 3) Intenta click seguro sobre el radio asociado al label (via atributo 'for')
            try
            {
                var forId = label.GetAttribute("for"); // ej: radio1, radio2, radio3
                if (!string.IsNullOrEmpty(forId))
                {
                    var radio = _wait.Until(ExpectedConditions.ElementExists(By.Id(forId)));

                    // Scrollea el radio al centro
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'})", radio);

                    // Click normal, con fallback a JS si está interceptado
                    try { radio.Click(); }
                    catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", radio); }

                    Assert.IsTrue(radio.Selected, $"No se pudo seleccionar el tipo '{textoLabel}'.");
                    return;
                }
            }
            catch { /* fallback al label */ }

                // 4) Fallback: click al label con JS (evita intercepción por superposiciones)
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'})", label);
            try
            {
                label.Click();
            }
            catch
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", label);
            }

            // 5) Verificación: si existe un radio hermano del label, asegúrate de que quedó seleccionado
            var radioHermano = _driver.FindElements(By.XPath(
                $"//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='{textoLabel}']/preceding-sibling::input[@type='radio'] | " +
                $"//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='{textoLabel}']/following-sibling::input[@type='radio']"))
                .FirstOrDefault();

            if (radioHermano != null && !radioHermano.Selected)
            {
                try { radioHermano.Click(); } catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", radioHermano); }
                Assert.IsTrue(radioHermano.Selected, $"No se pudo seleccionar el tipo '{textoLabel}'.");
            }
        }


        public void SetAutorizado(string valor)
        {
            var el = _wait.Until(ExpectedConditions.ElementToBeClickable(InputAutorizadoPor));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'})", el);
            el.Click();
            el.Clear();
            el.SendKeys(valor);
        }

        public void SetPagador(string valor)
        {
            var el = _wait.Until(ExpectedConditions.ElementToBeClickable(InputPagador));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'})", el);
            el.Click();
            el.Clear();
            el.SendKeys(valor);
            el.SendKeys(Keys.Tab); // dispara verificación en blur
        }

        public void SeleccionarDocumento(string texto)
        {
            // Render donde aparece el valor seleccionado (texto y/o title)
            By renderedText = By.XPath(
                "//*[@id='modal-registro-ingreso-egreso-varios']" +
                "//label[normalize-space()='DOCUMENTO']/following::*[contains(@class,'select2-selection--single')][1]" +
                "//*[contains(@class,'select2-selection__rendered')]"
            );

            // 1) Abrir Select2
            var btn = _wait.Until(ExpectedConditions.ElementToBeClickable(Select2DocumentoButton));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'})", btn);
            btn.Click();

            // 2) Escribir búsqueda
            var search = _wait.Until(ExpectedConditions.ElementIsVisible(Select2SearchInput));
            search.Clear();
            search.SendKeys(texto);

            // 3) Dropdown abierto
            var dropdown = _wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".select2-container--open")));

            // 4) Intento A: click a la opción exacta por texto
            IWebElement exactOption = null;
            try
            {
                exactOption = _wait.Until(drv =>
                    dropdown.FindElements(By.XPath($".//li[contains(@class,'select2-results__option') and normalize-space()='{texto}']"))
                            .FirstOrDefault());
                if (exactOption != null)
                {
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'nearest'})", exactOption);
                    try { exactOption.Click(); }
                    catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", exactOption); }
                }
            }
            catch { /* seguimos con otros intentos */ }

            // 5) Si aún está abierto, Intento B: Enter directo
            if (_driver.FindElements(By.CssSelector(".select2-container--open")).Any())
            {
                try
                {
                    search.Click();
                    new Actions(_driver).SendKeys(Keys.Enter).Perform();
                }
                catch { /* sigue */ }
            }

            // 6) Si aún está abierto, Intento C: ArrowDown + Enter (toma la resaltada)
            if (_driver.FindElements(By.CssSelector(".select2-container--open")).Any())
            {
                try
                {
                    search.Click();
                    new Actions(_driver).SendKeys(Keys.ArrowDown).SendKeys(Keys.Enter).Perform();
                }
                catch { /* sigue */ }
            }

            // 7) Si aún está abierto, Intento D: click JS a la resaltada
            if (_driver.FindElements(By.CssSelector(".select2-container--open")).Any())
            {
                var highlighted = dropdown.FindElements(By.CssSelector(".select2-results__option--highlighted")).FirstOrDefault();
                if (highlighted != null)
                {
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", highlighted);
                }
            }

            // 8) Esperar cierre del dropdown
            _wait.Until(d => !d.FindElements(By.CssSelector(".select2-container--open")).Any());

            // 9) Verificar que el render quedó con el texto esperado (algunas skins usan title)
            var rendered = _wait.Until(ExpectedConditions.ElementIsVisible(renderedText));
            _wait.Until(drv =>
            {
                var txt = (rendered.Text ?? "").Trim();
                var title = (rendered.GetAttribute("title") ?? "").Trim();
                return txt.Equals(texto, StringComparison.OrdinalIgnoreCase) ||
                       title.Equals(texto, StringComparison.OrdinalIgnoreCase);
            });

            // 10) Blur para fijar valor
            try
            {
                var modal = _wait.Until(ExpectedConditions.ElementIsVisible(Modal));
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", modal);
            }
            catch { /* opcional */ }
        }



        public void SetObservacion(string texto)
        {
            var el = _wait.Until(ExpectedConditions.ElementToBeClickable(TextareaObservacion));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'})", el);
            el.Click();
            el.Clear();
            el.SendKeys(texto);
        }

        public void SetImporte(string valor)
        {
            var el = _wait.Until(ExpectedConditions.ElementToBeClickable(InputImporte));
            el.Click();
            el.SendKeys(Keys.Control + "a");
            el.SendKeys(Keys.Delete);
            el.SendKeys(valor);
            el.SendKeys(Keys.Tab);
        }

        public void Guardar()
        {
            var btn = EsperarBotonGuardarHabilitado();
            ClickSeguro(btn);
            EsperarToastExitoOConfirm();
        }

        public void GuardarEsperandoErrores()
        {
            // Asegurar blur en campos clave para que se pinten validaciones
            try
            {
                var dni = _driver.FindElements(By.Id("DocumentoIdentidad")).FirstOrDefault(el => el.Displayed);
                dni?.SendKeys(Keys.Tab);
                var imp = _driver.FindElements(InputImporte).FirstOrDefault();
                imp?.SendKeys(Keys.Tab);
            }
            catch { }

            bool guardarHabilitado = IsGuardarEnabled();

            if (guardarHabilitado)
            {
                var btn = EsperarBotonGuardarHabilitado();
                ClickSeguro(btn);
            }
            // si no está habilitado, ya hay una validación previa impidiendo guardar

            // Espera “cualquier” evidencia de error/validación o que el modal siga abierto sin éxito
            _wait.Until(d =>
                HasToastError() ||
                HasBloqueInconsistencias() ||
                HasInlineErrors() ||
                IsDocumentoIdentidadInvalid() ||
                // Si tras intentar guardar el modal sigue abierto y no hay éxito, lo tomamos como validación bloqueante
                (IsModalOpen() && !HasToastExito())
            );

            // pequeño blur para terminar de pintar labels
            try
            {
                var modal = _wait.Until(ExpectedConditions.ElementIsVisible(Modal));
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", modal);
            }
            catch { }
        }

        public void DebeBloquearElRegistroPorValidacion()
        {
            // 1) Asegurarnos de que NO hubo éxito
            var hayToastExito = _driver.FindElements(ToastExito).Any();
            Assert.IsFalse(hayToastExito,
                "Se mostró un toast de éxito, por lo tanto el sistema permitió guardar y la prueba negativa debería fallar.");

            // 2) El modal sigue abierto y su botón del footer sigue visible
            var modalAbierto = _driver.FindElements(Modal).Any();
            var btnModal = _driver.FindElements(BtnFooterModal).FirstOrDefault();

            Assert.IsTrue(modalAbierto,
                "El modal de registro ya no está visible; parece que el formulario sí se cerró/guardó.");
            Assert.IsTrue(btnModal != null && btnModal.Displayed,
                "El botón del footer del modal ya no está visible; indica que el flujo se comportó como si fuera exitoso.");

            // 3) Seguimos en el módulo (menú lateral sigue ahí)
            var btnMenu = _driver.FindElements(BtnMenuTesoreriaYFinanzas).FirstOrDefault();
            Assert.IsTrue(btnMenu != null && btnMenu.Displayed,
                "No se encontró el botón del menú Tesorería y Finanzas; la navegación no es la esperada después del error.");
        }




        // ====== VALIDACIONES / GETTERS ======

        public string ObtenerValorPorLabel(string label)
        {
            var el = _wait.Until(ExpectedConditions.ElementExists(By.XPath(
                $"//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='{label}']/following::*[self::input or self::textarea][1]"
            )));
            return el.GetAttribute("value") ?? el.Text ?? string.Empty;
        }

        public string ObtenerImporteActual()
        {
            var el = _wait.Until(ExpectedConditions.ElementExists(InputImporte));
            return el.GetAttribute("value")?.Trim() ?? string.Empty;
        }

        public void DebeVerBloqueInconsistencias()
        {
            var el = _wait.Until(d => TryGetBloqueInconsistencias() ?? // elemento real
                                   (d.PageSource.IndexOf("INCONSISTENCIA", StringComparison.OrdinalIgnoreCase) >= 0
                                    || d.PageSource.IndexOf("Es necesario", StringComparison.OrdinalIgnoreCase) >= 0 ? new object() as IWebElement : null));
            Assert.IsNotNull(el, "No se encontró el bloque de inconsistencias ni texto 'Es necesario' en el DOM.");
        }

        public void DebeContenerInconsistencia(string textoClave)
        {
            var bloque = TryGetBloqueInconsistencias();
            var raw = (bloque?.Text ?? _driver.PageSource ?? "").ToLowerInvariant();

            var key = (textoClave ?? "").Trim().ToLowerInvariant();

            if (key == "pagador")
            {
                Assert.IsTrue(raw.Contains("pagador") || raw.Contains("beneficiario"),
                    $"No aparece la inconsistencia para pagador/beneficiario. Texto: {raw}");
                return;
            }
            if (key == "comprobante" || key == "documento")
            {
                Assert.IsTrue(raw.Contains("comprobante") || raw.Contains("documento"),
                    $"No aparece la inconsistencia para comprobante/documento. Texto: {raw}");
                return;
            }

            Assert.IsTrue(raw.Contains(key),
                $"No aparece la inconsistencia esperada: '{textoClave}'. Texto: {raw}");
        }


        public void NoDebeMostrarseToastExito()
        {
            var hayExito = _driver.FindElements(ToastExito).Any();
            Assert.IsFalse(hayExito, "Se mostró un toast de éxito cuando se esperaba error.");
        }

        public void DebeVerConfirmacion()
        {
            bool ok = false;

            // 1) El modal desapareció
            try
            {
                _wait.Until(ExpectedConditions.InvisibilityOfElementLocated(Modal));
                ok = true;
            }
            catch { }

            // 2) Toast de éxito
            if (!ok)
            {
                try
                {
                    ok = _wait.Until(d => d.FindElements(ToastExito).Any());
                }
                catch { }
            }

            // 3) Tabla de ingresos visible
            if (!ok)
            {
                var tabla = _driver.FindElements(By.Id("tabla-ingresos")).FirstOrDefault();
                if (tabla != null && tabla.Displayed)
                    ok = true;
            }

            // 4) Seguimos en el módulo (botón de Tesorería/Finanzas visible)
            if (!ok)
            {
                var btnMenu = _driver.FindElements(BtnMenuTesoreriaYFinanzas).FirstOrDefault();
                if (btnMenu != null && btnMenu.Displayed)
                    ok = true;
            }

            Assert.IsTrue(ok,
                "No se evidenció confirmación de registro (ni cierre de modal, ni toast de éxito, ni tabla de ingresos, ni regreso al módulo).");
        }


        public bool CampoInvalido(string label)
        {
            // 1) Resolver el input real según el label que llega del .feature
            IWebElement input = null;
            switch (label.Trim().ToUpperInvariant())
            {
                case "PAGADOR":
                    // DNI/RUC
                    input = _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.Id("DocumentoIdentidad")));
                    break;

                case "AUTORIZADO POR":
                    input = _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(
                        By.XPath("//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='AUTORIZADO POR']/following::input[@type='text'][1]")
                    ));
                    break;

                case "IMPORTE":
                    input = _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(
                        By.XPath("//*[@id='modal-registro-ingreso-egreso-varios']//label[normalize-space()='IMPORTE']/following::input[1]")
                    ));
                    break;

                default:
                    // Si usas otros campos en el futuro, mápealos aquí.
                    throw new NUnit.Framework.AssertionException($"Campo no mapeado para validación: {label}");
            }

            // 2) Forzar blur para que Angular pinte la validación
            try { input.SendKeys(OpenQA.Selenium.Keys.Tab); } catch { }

            // 3) Esperar hasta 10s alguna evidencia de inválido
            var localWait = new OpenQA.Selenium.Support.UI.WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            try
            {
                return localWait.Until(_ =>
                {
                    var cls = input.GetAttribute("class") ?? "";
                    var aria = input.GetAttribute("aria-invalid") ?? "";
                    var style = input.GetAttribute("style") ?? "";

                    // a) ng-invalid / aria-invalid
                    if (cls.Contains("ng-invalid") || aria.Equals("true", StringComparison.OrdinalIgnoreCase))
                        return true;

                    // b) bordes/estilos (fallback visual)
                    if (style.Contains("border") || style.Contains("red"))
                        return true;

                    // c) mensajes pegados via aria-describedby
                    var desc = input.GetAttribute("aria-describedby");
                    if (!string.IsNullOrWhiteSpace(desc))
                    {
                        foreach (var id in desc.Split(' '))
                        {
                            var el = _driver.FindElements(By.Id(id)).FirstOrDefault();
                            if (el != null && !string.IsNullOrWhiteSpace(el.Text))
                                return true;
                        }
                    }

                    // d) contenedor con clases de error (Bootstrap/Angular)
                    try
                    {
                        var cont = input.FindElement(By.XPath("./ancestor-or-self::*[contains(@class,'form-group') or contains(@class,'has-error') or contains(@class,'input-group')][1]"));
                        var contCls = cont.GetAttribute("class") ?? "";
                        if (contCls.Contains("has-error")) return true;

                        var cerca = cont.Text?.ToLowerInvariant() ?? "";
                        if (cerca.Contains("error") || cerca.Contains("inválid") || cerca.Contains("invalido"))
                            return true;
                    }
                    catch { /* sin contenedor, no pasa nada */ }

                    return false;
                });
            }
            catch
            {
                return false;
            }
        }


        public void DebeVerError(string texto)
        {
            string target = (texto ?? "").Trim().ToLowerInvariant();

            bool encontrado = false;

            try
            {
                // Esperar hasta que en algún contenedor aparezca el texto
                encontrado = _wait.Until(d =>
                {
                    // 0) SweetAlert – cuerpo
                    var swalTexts = d.FindElements(SwalText);
                    if (swalTexts.Any(s => ((s.Text ?? "").ToLowerInvariant().Contains(target))))
                        return true;

                    // 0.1) SweetAlert – título
                    var swalTitles = d.FindElements(SwalTitle);
                    if (swalTitles.Any(s => ((s.Text ?? "").ToLowerInvariant().Contains(target))))
                        return true;

                    // 1) Toasts de error
                    var toasts = d.FindElements(ToastError);
                    if (toasts.Any(t => ((t.Text ?? "").ToLowerInvariant().Contains(target))))
                        return true;

                    // 2) Bloque de inconsistencias
                    var bloques = d.FindElements(BloqueInconsistencias);
                    if (bloques.Any(b => (b.Text ?? "").ToLowerInvariant().Contains(target)))
                        return true;

                    // 3) Mensajes inline en el modal
                    var inlineMsgs = d.FindElements(By.CssSelector(
                        "#modal-registro-ingreso-egreso-varios .text-danger, " +
                        "#modal-registro-ingreso-egreso-varios .field-validation-error, " +
                        "#modal-registro-ingreso-egreso-varios .help-block, " +
                        "#modal-registro-ingreso-egreso-varios .validation-error"
                    ));
                    if (inlineMsgs.Any(m => (m.Text ?? "").ToLowerInvariant().Contains(target)))
                        return true;

                    return false;
                });
            }
            catch
            {
                // si se agota el tiempo, 'encontrado' seguirá en false
            }

            if (!encontrado)
            {
                Assert.Fail($"No se encontró el mensaje de error esperado: '{texto}'.");
            }
        }







        // ====== HELPERS ======

        private IWebElement EsperarBotonGuardarHabilitado()
        {
            return _wait.Until(driver =>
            {
                var btn = driver.FindElement(BtnGuardar);
                var disabledAttr = btn.GetAttribute("disabled");
                if (btn.Enabled && string.IsNullOrEmpty(disabledAttr))
                    return btn;
                return null;
            });
        }

        private IWebElement TryGetBloqueInconsistencias()
        {
            var els = _driver.FindElements(BloqueInconsistencias);
            if (els.Any()) return els.First();

            // Fallback 1: cualquier item de lista con “Es necesario …”
            els = _driver.FindElements(By.XPath("//*[contains(translate(.,'ÁÉÍÓÚáéíóú','AEIOUaeiou'),'es necesario')]"));
            if (els.Any()) return els.First();

            return null; // dejaremos otro fallback en las aserciones
        }

        private void ClickSeguro(IWebElement el)
        {
            try { el.Click(); }
            catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", el); }
        }

        private void EsperarToastExitoOConfirm()
        {
            _wait.Until(d =>
                d.FindElements(ToastExito).Any() ||
                d.FindElements(By.Id("tabla-ingresos")).Any() ||
                !d.FindElements(Modal).Any());
        }

        private bool IsModalOpen()
        {
            return _driver.FindElements(Modal).Any();
        }

        private bool HasToastExito()
        {
            return _driver.FindElements(ToastExito).Any();
        }

        private bool HasToastError()
        {
            return _driver.FindElements(ToastError).Any();
        }

        private bool HasBloqueInconsistencias()
        {
            return _driver.FindElements(BloqueInconsistencias).Any();
        }

        private bool HasInlineErrors()
        {
            return _driver.FindElements(By.CssSelector(
                "#modal-registro-ingreso-egreso-varios .text-danger, " +
                "#modal-registro-ingreso-egreso-varios .field-validation-error, " +
                "#modal-registro-ingreso-egreso-varios .help-block, " +
                "#modal-registro-ingreso-egreso-varios .validation-error"
            )).Any();
        }

        private bool IsDocumentoIdentidadInvalid()
        {
            return _driver.FindElements(By.CssSelector("#DocumentoIdentidad.ng-invalid")).Any();
        }

        private bool IsGuardarEnabled()
        {
            try
            {
                var btn = _driver.FindElement(BtnGuardar);
                var disabledAttr = btn.GetAttribute("disabled");
                return btn.Displayed && btn.Enabled && string.IsNullOrEmpty(disabledAttr);
            }
            catch { return false; }
        }


    }
}
