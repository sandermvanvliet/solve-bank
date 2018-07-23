using System;

namespace SolveBank.Models
{
    public class Transaction
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public long Sequence { get; set; }
        public decimal Amount { get; }
        public string Currency { get; }

        public Transaction(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }
    }
}