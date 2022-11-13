using System;

namespace MicroRabbit.Domain.Core.Events
{
    public abstract class Event
    { 
        public DateTime Timestamp { get; }

        protected Event()
        {
            Timestamp = DateTime.Now;
        }
    }
}
