using System;

namespace SolveBank.Exceptions
{
    public class InvalidTransferException : Exception
    {
        public InvalidTransferException(string reason)
            : base($"Transfer is not valid: {reason}")
        {
            Data.Add(nameof(reason), reason);
        }
    }
}