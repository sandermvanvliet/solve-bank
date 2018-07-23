using System.Collections.Generic;

namespace SolveBank.Models
{
    public class BankAccount
    {
        public string AccountNumber { get; set; }
        public decimal Balance { get; private set; }
        public string Currency { get; set; }
        private readonly List<Transaction> _transactions = new List<Transaction>();
        private readonly object _syncRoot = new object();

        public void Deposit(decimal amount, string currency)
        {
            lock (_syncRoot)
            {
                var nextSequenceNumber = _transactions.Count;

                var transaction = new Deposit(amount, currency)
                {
                    Sequence = nextSequenceNumber
                };

                _transactions.Add(transaction);

                Balance = Balance += amount;
            }
        }

        public void Withdraw(decimal amount, string currency)
        {
            lock (_syncRoot)
            {
                var nextSequenceNumber = _transactions.Count;

                var transaction = new Withdrawal(amount, currency)
                {
                    Sequence = nextSequenceNumber
                };

                _transactions.Add(transaction);

                Balance = Balance -= amount;
            }
        }
    }
}