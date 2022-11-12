namespace MicroRabbit.Banking.Domain.Commands
{
    public sealed class CreateTransferCommand : TransferCommand
    {
        public CreateTransferCommand(int from, int to, decimal amount)
        {
            From = from;
            To = to;
            Amount = amount;
        }
    }
}
