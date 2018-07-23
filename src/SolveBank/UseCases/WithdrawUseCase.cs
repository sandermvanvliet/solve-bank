using SolveBank.Exceptions;
using SolveBank.Ports.Persistence;

namespace SolveBank.UseCases
{
    public class WithdrawUseCase
    {
        private readonly IBankAccountStore _bankAccountStore;

        public WithdrawUseCase(IBankAccountStore bankAccountStore)
        {
            _bankAccountStore = bankAccountStore;
        }

        public void WithdrawFrom(string accountNumber, decimal amount, string currency)
        {
            if (amount < 0)
            {
                throw new InvalidWithDrawalException("Amount cannot be less than 0");
            }

            var bankAccount = _bankAccountStore.GetByAccountNumber(accountNumber);

            if (bankAccount == null)
            {
                throw new AccountNotFoundException(accountNumber);
            }

            if ((bankAccount.Balance - amount) < 0)
            {
                throw new InsufficientBalanceException();
            }
            
            bankAccount.Withdraw(amount, currency);
        }
    }
}