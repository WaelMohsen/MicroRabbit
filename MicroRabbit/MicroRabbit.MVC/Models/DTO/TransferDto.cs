namespace MicroRabbit.MVC.Models.DTO
{
    public sealed class TransferDto
    {
        public int FromAccount { get; set; }
        public int ToAccount { get; set; }
        public decimal TransferAmount { get; set; }
    }
}
