using SimpleInjector;
using SolveBank.Adapters.Authorisation.Ports.Authorisation;
using SolveBank.Ports.Authorisation;

namespace SolveBank.Adapters.Authorisation
{
    public class AuthorisationAdapter
    {
        public static void Register(Container container)
        {
            var adapterContainer = new Container();

            adapterContainer.RegisterSingleton<IAccountAuthorisation, AlwaysSuccessfulAccountAuthorisation>();

            // Proxy registrations for the host container
            container.Register(() => adapterContainer.GetInstance<IAccountAuthorisation>());
        }
    }
}
