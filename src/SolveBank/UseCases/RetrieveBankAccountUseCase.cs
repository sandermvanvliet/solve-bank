using System;
using SolveBank.Exceptions;
using SolveBank.Models;
using SolveBank.Ports.Persistence;

namespace SolveBank.UseCases
{
    public class RetrieveBankAccountUseCase
    {
        private readonly IBankAccountStore _bankAccountStore;

        public RetrieveBankAccountUseCase(IBankAccountStore bankAccountStore)
        {
            _bankAccountStore = bankAccountStore;
        }

        public BankAccount GetByAccountNumber(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber))
            {
                throw new ArgumentNullException(nameof(accountNumber));
            }

            var bankAccount = _bankAccountStore.GetByAccountNumber(accountNumber);

            if (bankAccount == null)
            {
                throw new AccountNotFoundException(accountNumber);
            }

            return bankAccount;
        }
    }
}