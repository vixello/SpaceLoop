using Assets.Scripts.Core.Interfaces;
using VContainer;

namespace Assets.Scripts.Core
{
    public class CommandFactory<TCommand> : ICommandFactory<TCommand>
    {
        private readonly IObjectResolver _resolver;

        public CommandFactory(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public TCommand Create()
        {
            return _resolver.Resolve<TCommand>();
        }
    }
}
