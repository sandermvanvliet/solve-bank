using System;
using SimpleInjector;
using SolveBank.Adapters.Authorisation;
using SolveBank.Adapters.Persistence;

namespace SolveBank.Console
{
    internal class Bootstrapper
    {
        public static Container BootstrapContainerAndAdapters(Action<Container> registerOverrides = null)
        {
            var container = new Container();
            
            AuthorisationAdapter.Register(container);
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