using SolveBank.Models;

namespace SolveBank.Ports.Persistence
{
    public interface IBankAccountStore
    {
        BankAccount GetByAccountNumber(string accountNumber);
    }
}