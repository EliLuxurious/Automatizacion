using System;
using Reqnroll;

namespace Automation.StepDefinitions
{
    [Binding]
    public class LoginFeatureStepDefinitions
    {
        [When("accede el módulo {string}")]
        public void WhenAccedeElModulo(string p0)
        {
            throw new PendingStepException();
        }

        [When("accede al submódulo {string}")]
        public void WhenAccedeAlSubmodulo(string p0)
        {
            throw new PendingStepException();
        }
    }
}
