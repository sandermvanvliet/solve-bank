using SimpleInjector;
using SolveBank.Adapters.InMemory;

namespace SolveBank.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = BootstrapContainerAndAdapters();

            System.Console.WriteLine("Hello World!");
        }

        private static Container BootstrapContainerAndAdapters()
        {
            var container = new Container();

            InMemoryAdapter.Register(container);

            container.Verify();

            return container;
        }
    }
}
