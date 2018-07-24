using SimpleInjector;
using SolveBank.Adapters.Persistence.Ports.Persistence;
using SolveBank.Ports.Persistence;

namespace SolveBank.Adapters.Persistence
{
    public class InMemoryAdapter
    {
        public static void Register(Container container)
        {
            var adapterContainer = new Container();

            adapterContainer.RegisterSingleton<IBankAccountStore, InMemoryBankAccountStore>();

            // Proxy registrations for the host container
            container.Register(() => adapterContainer.GetInstance<IBankAccountStore>());
        }
    }
}
