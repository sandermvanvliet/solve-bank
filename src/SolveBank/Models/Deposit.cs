namespace SolveBank.Models
{
    public class Deposit : Transaction
    {
        public Deposit(decimal amount, string currency)
            : base(amount,
                currency)
        {
        }
    }
}