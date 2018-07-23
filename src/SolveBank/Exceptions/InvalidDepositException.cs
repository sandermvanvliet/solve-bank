using System;

namespace SolveBank.Exceptions
{
    public class InvalidDepositException : Exception
    {
        public InvalidDepositException(string reason)
            : base($"Deposit is not valid: {reason}")
        {
            Data.Add(nameof(reason), reason);
        }
    }
}