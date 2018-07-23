namespace SolveBank.Models
{
    public class Withdrawal : Transaction
    {
        public Withdrawal(decimal amount, string currency)
            : base(amount, currency)
        {
        }
    }
}