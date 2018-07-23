using System;

namespace SolveBank.Exceptions
{
    public class InsufficientBalanceException : Exception
    {
        public InsufficientBalanceException()
            : base("Account balance not sufficient")
        {
        }
    }
}