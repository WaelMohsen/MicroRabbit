using MicroRabbit.Transfer.Data.Context;
using MicroRabbit.Transfer.Domain.Interfaces;
using MicroRabbit.Transfer.Domain.Models;
using System.Collections.Generic;

namespace MicroRabbit.Transfer.Data.Repository
{
    public sealed class TransferRepository : ITransferRepository
    {
        private TransferDbContext _dbContext;

        public TransferRepository(TransferDbContext ctx)
        {
            _dbContext = ctx;
        }

        public void Add(TransferLog transferLog)
        {
            _dbContext.TransferLogs.Add(transferLog);
            _dbContext.SaveChanges();
        }

        public IEnumerable<TransferLog> GetTransferLogs()
        {
            return _dbContext.TransferLogs;
        }
    }
}
