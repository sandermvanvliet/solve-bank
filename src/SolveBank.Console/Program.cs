using System;
using Oakton;
using SimpleInjector;

namespace SolveBank.Console
{
    public class Program
    {
        private readonly CommandExecutor _executor;

        private static void Main(string[] args)
        {
            var program = new Program();

            var exitCode = program.Run(args);

            Environment.Exit(exitCode);
        }

        public Program()
            : this(null)
        {
        }

        internal Program(Action<Container> registerOverrides = null)
        {
            var container = Bootstrapper.BootstrapContainerAndAdapters(registerOverrides);

            _executor = CommandExecutor
                .For(_ => _.RegisterCommands(typeof(Program).Assembly),
                    new SimpleInjectorCommandCreator(container));
        }

        public int Run(string[] args)
        {
            try
            {
                return _executor.Execute(args);
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine($"Oops! Something went very wrong, I'm sorry. Please show this message to your friendly neighbourhood developer: {ex.Message}");
                
                return 1;
            }
        }
    }
}
