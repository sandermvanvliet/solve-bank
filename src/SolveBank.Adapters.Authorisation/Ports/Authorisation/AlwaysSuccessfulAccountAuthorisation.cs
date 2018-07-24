using SolveBank.Models;
using SolveBank.Ports.Authorisation;

namespace SolveBank.Adapters.Authorisation.Ports.Authorisation
{
    internal class AlwaysSuccessfulAccountAuthorisation : IAccountAuthorisation
    {
        public bool RequestForWithdrawal(BankAccount bankAccount)
        {
            return true;
        }

        public bool RequestForDeposit(BankAccount bankAccount)
        {
            return true;
        }
    }
}