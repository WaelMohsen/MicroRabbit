using MicroRabbit.Domain.Core.Events;

namespace MicroRabbit.Banking.Domain.Events
{
    public sealed class TransferCreatedEvent : Event
    {
        private int From { get; set; }

        private int To { get;  set; }

        private decimal Amount { get; set; }

        public TransferCreatedEvent(int from, int to, decimal amount)
        {
            From = from;
            To = to;
            Amount = amount;
        }
    }
}
