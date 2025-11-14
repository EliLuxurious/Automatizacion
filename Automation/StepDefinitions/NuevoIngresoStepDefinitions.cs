using Automation.Pages;                 // LoginPage, MenuPage (si están allí)
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using RegistroIngreso.Pages;            // IngresoPage
using Reqnroll;

namespace Automation.StepDefinitions
{
    [Binding]
    public class NuevoIngresoStepDefinitions
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private readonly LoginPage _login;
        private readonly MenuPage _menu;
        private readonly IngresoPage _ingresos;

        // El driver y el wait vienen de Hooks (BoDi)
        public NuevoIngresoStepDefinitions(IWebDriver driver, WebDriverWait wait)
        {
            _driver = driver;
            _wait = wait;
            _login = new LoginPage(_driver, _wait);
            _menu = new MenuPage(_driver, _wait);
            _ingresos = new IngresoPage(_driver, _wait);
        }

        // No crees ni cierres driver aquí: Hooks lo hace.
        // [BeforeScenario] y [AfterScenario] ya no son necesarios.

        [Given("el usuario ingresa al ambiente {string}")]
        public void GivenAmbiente(string url) => _login.GoTo(url);

        [When("el usuario ingresa sesion con usuario {string} y contraseña {string}")]
        public void WhenLogin(string u, string p) => _login.SignIn(u, p);

        [When("acceder al modulo {string}")]
        public void WhenModulo(string m) => _menu.OpenModule(m);

        [When("acceder al submodulo {string}")]
        public void WhenSubmodulo(string s) => _menu.OpenSubmodule(s);

        [When("el usuario hace clic en el botón {string}")]
        public void WhenClickIngreso(string btn)
        {
            if (btn.Trim().ToUpper() == "INGRESO")
                _ingresos.AbrirFormularioIngreso();
        }

        [When("completa el campo {string} con {string}")]
        public void WhenCompletaCampo(string campo, string valor)
        {
            // si el ejemplo manda vacío, no escribir nada (deja el campo faltante)
            if (string.IsNullOrWhiteSpace(valor))
                return;

            switch (campo.Trim().ToUpperInvariant())
            {
                case "AUTORIZADO POR": _ingresos.SetAutorizado(valor); break;
                case "PAGADOR": _ingresos.SetPagador(valor); break;
                case "DOCUMENTO":
                case "TIPO DE DOCUMENTO":
                case "DOC":
                    _ingresos.SeleccionarDocumento(valor); break;
                case "IMPORTE": _ingresos.SetImporte(valor); break;
                case "OBSERVACIÓN":
                case "OBSERVACION": _ingresos.SetObservacion(valor); break;
                //case "SERIE": _ingresos.SetSerie(valor); break;
                //case "CORRELATIVO": _ingresos.SetCorrelativo(valor); break;
                default: Assert.Fail($"Campo no soportado: {campo}"); break;

            }
        }


        [When("selecciona {string} como tipo de persona")]
        public void WhenTipoPersona(string t) => _ingresos.SeleccionarTipoPersona(t);

        [When("selecciona el tipo de documento {string}")]
        public void WhenDoc(string d) => _ingresos.SeleccionarDocumento(d);

        [When("completa el importe con {string}")]
        public void WhenCompletaElImporteCon(string monto) => _ingresos.SetImporte(monto);

        [When("escribe la observación {string}")]
        public void WhenEscribeLaObservacion(string obs) => _ingresos.SetObservacion(obs);


        [When("presiona el botón {string}")]
        public void WhenGuardar(string b)
        {
            if (b.Trim().ToUpper() == "GUARDAR") _ingresos.Guardar();
        }

        [When("intenta guardar el ingreso")]
        public void WhenIntentaGuardarElIngreso()
        {
            _ingresos.GuardarEsperandoErrores(); // método negativo en tu POM
        }

        // 1) Generar observación larga sin pegarla en el .feature
        [When(@"escribe una observación de '(\d+)' caracteres")]
        public void WhenEscribeUnaObservacionDeNCaracteres(int n)
        {
            if (n < 1) n = 1;
            var obs = new string('X', n);
            _ingresos.SetObservacion(obs);
        }

        // 2) Verificar que un campo (por etiqueta) quedó vacío
        [Then(@"el campo '(.*)' queda vacío")]
        public void ThenElCampoQuedaVacio(string label)
        {
            var val = _ingresos.ObtenerValorPorLabel(label);
            Assert.IsTrue(string.IsNullOrWhiteSpace(val), $"Se esperaba '{label}' vacío, pero tiene: '{val}'.");
        }

        // 3) Verificar normalización/formato del importe
        [Then(@"el importe formateado es '(.*)'")]
        public void ThenElImporteFormateadoEs(string esperado)
        {
            var val = _ingresos.ObtenerImporteActual();
            Assert.AreEqual(esperado, val, $"Importe formateado esperado '{esperado}', actual '{val}'.");
        }


        [Then("se muestran las inconsistencias requeridas")]
        public void ThenSeMuestranLasInconsistenciasReq(Reqnroll.Table table)
        {
            _ingresos.DebeVerBloqueInconsistencias();
            foreach (var row in table.Rows)
            {
                _ingresos.DebeContenerInconsistencia(row["mensaje"]);
            }
            _ingresos.NoDebeMostrarseToastExito();
        }


        [Then("el ingreso se registra correctamente")]
        public void ThenOk() => _ingresos.DebeVerConfirmacion();

        [Then("se muestra el mensaje de error '(.*)'")]
        public void ThenErr(string msg) => _ingresos.DebeVerError(msg);

        [Then(@"el campo '(.*)' queda inválido")]
        public void ThenCampoQuedaInvalido(string label)
        {
            Assert.IsTrue(_ingresos.CampoInvalido(label),
                $"Se esperaba el campo '{label}' inválido y no lo está.");
        }

        [Then(@"no debe permitir registrar el ingreso")]
        public void ThenNoDebePermitirRegistrarElIngreso()
        {
            // Ya hiciste: _ingresoPage.GuardarEsperandoErrores();
            _ingresos.DebeBloquearElRegistroPorValidacion();
        }


        [Then(@"el bloque de inconsistencias contiene")]
        public void ThenBloqueDeInconsistenciasContiene(Reqnroll.Table table)
        {
            _ingresos.DebeVerBloqueInconsistencias();
            foreach (var row in table.Rows)
            {
                var texto = row["texto"];            // columna de tu tabla en el .feature
                _ingresos.DebeContenerInconsistencia(texto);
            }
            _ingresos.NoDebeMostrarseToastExito();
        }




    }
}
