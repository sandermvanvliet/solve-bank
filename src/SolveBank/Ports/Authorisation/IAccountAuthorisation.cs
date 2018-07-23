using SolveBank.Models;

namespace SolveBank.Ports.Authorisation
{
    public interface IAccountAuthorisation
    {
        bool RequestForWithdrawal(BankAccount bankAccount);
        bool RequestForDeposit(BankAccount bankAccount);
    }
}