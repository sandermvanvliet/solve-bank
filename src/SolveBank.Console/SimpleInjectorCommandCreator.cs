using System;
using Oakton;
using SimpleInjector;

namespace SolveBank.Console
{
    internal class SimpleInjectorCommandCreator : ICommandCreator
    {
        private readonly Container _container;

        public SimpleInjectorCommandCreator(Container container)
        {
            _container = container;
        }

        public IOaktonCommand CreateCommand(Type commandType)
        {
            return (IOaktonCommand) _container.GetInstance(commandType);
        }

        public object CreateModel(Type modelType)
        {
            return _container.GetInstance(modelType);
        }
    }
}