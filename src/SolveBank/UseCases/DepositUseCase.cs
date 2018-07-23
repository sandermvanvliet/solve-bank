using SolveBank.Exceptions;
using SolveBank.Ports.Authorisation;
using SolveBank.Ports.Persistence;

namespace SolveBank.UseCases
{
    public class DepositUseCase
    {
        private readonly IBankAccountStore _bankAccountStore;
        private readonly IAccountAuthorisation _accountAuthorisation;

        public DepositUseCase(IBankAccountStore bankAccountStore, IAccountAuthorisation accountAuthorisation)
        {
            _bankAccountStore = bankAccountStore;
            _accountAuthorisation = accountAuthorisation;
        }

        public void DepositTo(string accountNumber, decimal amount, string currency)
        {
            if (amount < 0)
            {
                throw new InvalidDepositException("Amount cannot be less than 0");
            }

            var bankAccount = _bankAccountStore.GetByAccountNumber(accountNumber);
            
            if (bankAccount == null)
            {
                throw new AccountNotFoundException(accountNumber);
            }

            if (!_accountAuthorisation.RequestForDeposit(bankAccount))
            {
                throw new AuthorisationFailedException();
            }

            bankAccount.Deposit(amount, currency);
        }
    }
}