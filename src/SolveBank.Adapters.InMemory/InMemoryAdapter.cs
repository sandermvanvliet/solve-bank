using SimpleInjector;
using SolveBank.Adapters.InMemory.Ports.Authorisation;
using SolveBank.Adapters.InMemory.Ports.Persistence;
using SolveBank.Ports.Authorisation;
using SolveBank.Ports.Persistence;

namespace SolveBank.Adapters.InMemory
{
    public class InMemoryAdapter
    {
        public static void Register(Container container)
        {
            var adapterContainer = new Container();

            adapterContainer.Register<IAccountAuthorisation, AlwaysSuccessfulAccountAuthorisation>();
            adapterContainer.Register<IBankAccountStore, InMemoryBankAccountStore>();

            // Proxy registrations for the host container
            container.Register(() => adapterContainer.GetInstance<IAccountAuthorisation>());
            container.Register(() => adapterContainer.GetInstance<IBankAccountStore>());
        }
    }
}
