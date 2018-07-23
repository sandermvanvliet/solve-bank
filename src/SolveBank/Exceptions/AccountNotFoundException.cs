using System;

namespace SolveBank.Exceptions
{
    public class AccountNotFoundException : Exception
    {
        public AccountNotFoundException(string accountNumber)
            : base((string) "Account could not be found")
        {
            Data.Add(nameof(accountNumber), accountNumber);
        }
    }
}