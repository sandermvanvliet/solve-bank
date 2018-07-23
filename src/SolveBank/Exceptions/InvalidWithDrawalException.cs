using System;

namespace SolveBank.Exceptions
{
    public class InvalidWithDrawalException : Exception
    {
        public InvalidWithDrawalException(string reason)
            : base($"Withdrawal is not valid: {reason}")
        {
            Data.Add(nameof(reason), reason);
        }
    }
}