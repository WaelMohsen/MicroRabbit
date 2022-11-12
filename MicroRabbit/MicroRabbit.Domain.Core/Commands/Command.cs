using MicroRabbit.Domain.Core.Events;
using System;

namespace MicroRabbit.Domain.Core.Commands
{
    public abstract class Command : Message
    {
        private DateTime Timestamp { get; set; }

        protected Command()
        {
            Timestamp = DateTime.Now;
        }
    }
}
