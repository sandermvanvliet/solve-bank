using System;
using Oakton;
using SimpleInjector;
using SolveBank.Adapters.InMemory;

namespace SolveBank.Console
{
    public class Program
    {
        private readonly CommandExecutor _executor;

        static void Main(string[] args)
        {
            var program = new Program();

            program.Run(args);
        }

        public Program()
        {
            var container = BootstrapContainerAndAdapters();

            _executor = CommandExecutor
                .For(_ => _.RegisterCommands(typeof(Program).Assembly),
                    new SimpleInjectorCommandCreator(container));
        }

        private void Run(string[] args)
        {
            try
            {
                _executor.Execute(args);
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine($"Oops! Something went very wrong, I'm sorry. Please show this message to your friendly neighbourhood developer: {ex.Message}");
                Environment.Exit(1);
            }
        }

        private Container BootstrapContainerAndAdapters()
        {
            var container = new Container();

            InMemoryAdapter.Register(container);

            ApplyContainerOverrides();

            container.Verify();

            return container;
        }

        protected virtual void ApplyContainerOverrides()
        {
        }
    }
}
