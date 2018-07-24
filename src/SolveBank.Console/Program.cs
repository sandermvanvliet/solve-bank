using Oakton;
using SimpleInjector;
using SolveBank.Adapters.InMemory;

namespace SolveBank.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = BootstrapContainerAndAdapters();

            var executor = CommandExecutor.For(_ =>
                    _.RegisterCommands(typeof(Program).Assembly),
                new SimpleInjectorCommandCreator(container));

            executor.Execute(args);
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
