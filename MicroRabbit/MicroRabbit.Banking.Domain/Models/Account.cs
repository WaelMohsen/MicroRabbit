namespace MicroRabbit.Banking.Domain.Models
{
    public sealed class Account
    {
        public int Id { get; set; }
        public string AccountType { get; set; }
        public decimal AccountBalance { get; set; }
    }
}
