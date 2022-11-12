using MicroRabbit.Banking.Data.Context;
using MicroRabbit.Banking.Domain.Interfaces;
using MicroRabbit.Banking.Domain.Models;
using System.Collections.Generic;

namespace MicroRabbit.Banking.Data.Repository
{
    public sealed class AccountRepository : IAccountRepository
    {
        private BankingDbContext _bankingDbContext;

        public AccountRepository(BankingDbContext ctx)
        {
            _bankingDbContext = ctx;
        }

        public IEnumerable<Account> GetAccounts()
        {
            return _bankingDbContext.Accounts;
        }
    }
}
