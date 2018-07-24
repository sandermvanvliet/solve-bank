using SimpleInjector;
using SolveBank.Adapters.Authorisation.Ports.Authorisation;
using SolveBank.Ports.Authorisation;

namespace SolveBank.Adapters.Authorisation
{
    public class AuthorisationAdapter
    {
        public static void Register(Container container, FeatureToggles featureToggles)
        {
            var adapterContainer = new Container();

            if (featureToggles.UseAlwaySuccessfulAuthorisation)
            {
                adapterContainer.RegisterSingleton<IAccountAuthorisation, AlwaysSuccessfulAccountAuthorisation>();
            }
            else if (featureToggles.UseConsoleAuthorisation)
            {
                adapterContainer.RegisterSingleton<IAccountAuthorisation, ConsoleAccountAuthorisation>();
            }

            // Proxy registrations for the host container
            container.Register(() => adapterContainer.GetInstance<IAccountAuthorisation>());
        }
    }
}
