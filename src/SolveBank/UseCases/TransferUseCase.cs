﻿using SolveBank.Exceptions;
using SolveBank.Ports.Authorisation;
using SolveBank.Ports.Persistence;

namespace SolveBank.UseCases
{
    public class TransferUseCase
    {
        private readonly IBankAccountStore _bankAccountStore;
        private readonly IAccountAuthorisation _accountAuthorisation;

        public TransferUseCase(IBankAccountStore bankAccountStore, IAccountAuthorisation accountAuthorisation)
        {
            _bankAccountStore = bankAccountStore;
            _accountAuthorisation = accountAuthorisation;
        }

        public void Transfer(decimal amount, string currency, string sourceAccountNumber, string destinationAccountNumber)
        {
            if (amount < 0)
            {
                throw new InvalidTransferException("Amount cannot be less than 0");
            }

            var sourceAccount = _bankAccountStore.GetByAccountNumber(sourceAccountNumber);

            if (sourceAccount == null)
            {
                throw new AccountNotFoundException(sourceAccountNumber);
            }

            var destinationAccount = _bankAccountStore.GetByAccountNumber(destinationAccountNumber);

            if (destinationAccount == null)
            {
                throw new AccountNotFoundException(destinationAccountNumber);
            }

            if (!_accountAuthorisation.RequestForWithdrawal(sourceAccount))
            {
                throw new AuthorisationFailedException();
            }

            if ((sourceAccount.Balance - amount) < 0)
            {
                throw new InsufficientBalanceException();
            }

            sourceAccount.Withdraw(amount, currency);
            destinationAccount.Deposit(amount, currency);
        }
    }
}