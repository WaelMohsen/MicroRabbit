using MediatR;

namespace MicroRabbit.Domain.Core.Events
{
    public abstract class Message : IRequest<bool>
    {
        private string MessageType { get; set; }

        protected Message()
        {
            MessageType = GetType().Name;
        }
    }
}
