using System;

namespace MicroRabbit.Domain.Core.Events
{
    public abstract class Event
    {
        private DateTime Timestamp { get; set; }

        protected Event()
        {
            Timestamp = DateTime.Now;
        }
    }
}
