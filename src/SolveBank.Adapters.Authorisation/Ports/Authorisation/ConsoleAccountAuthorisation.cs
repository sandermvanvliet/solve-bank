using System;
using SolveBank.Models;
using SolveBank.Ports.Authorisation;

namespace SolveBank.Adapters.Authorisation.Ports.Authorisation
{
    public class ConsoleAccountAuthorisation : IAccountAuthorisation
    {
        public bool RequestForWithdrawal(BankAccount bankAccount)
        {
            return AskSecurityQuestion(bankAccount);
        }

        public bool RequestForDeposit(BankAccount bankAccount)
        {
            return AskSecurityQuestion(bankAccount);
        }

        private static bool AskSecurityQuestion(BankAccount bankAccount)
        {
            Console.WriteLine(
                $"Please ask the customer the security question: '{bankAccount.Customer.SecurityQuestion}', the answer should be: {bankAccount.Customer.SecurityAnswer}");
            Console.WriteLine("If the answer is correct, type 'correct' and press enter. Otherwise press enter");

            var response = Console.ReadLine()
                ?.Trim();

            if (string.IsNullOrWhiteSpace(response))
            {
                return false;
            }

            return "yes".Equals(response,
                StringComparison.InvariantCultureIgnoreCase);
        }
    }
}