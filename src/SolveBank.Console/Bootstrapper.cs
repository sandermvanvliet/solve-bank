using System;
using SimpleInjector;
using SolveBank.Adapters.InMemory;

namespace SolveBank.Console
{
    internal class Bootstrapper
    {
        public static Container BootstrapContainerAndAdapters(Action<Container> registerOverrides = null)
        {
            var container = new Container();

            InMemoryAdapter.Register(container);

            if (registerOverrides != null)
            {
                container.Options.AllowOverridingRegistrations = true;
                registerOverrides(container);
            }

            container.Verify();

            return container;
        }
    }
}