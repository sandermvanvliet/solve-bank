using SolveBank.Exceptions;
using SolveBank.Ports.Persistence;

namespace SolveBank.UseCases
{
    public class DepositUseCase
    {
        private readonly IBankAccountStore _bankAccountStore;

        public DepositUseCase(IBankAccountStore bankAccountStore)
        {
            _bankAccountStore = bankAccountStore;
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

            bankAccount.Deposit(amount, currency);
        }
    }
}