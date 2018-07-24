using Oakton;

namespace SolveBank.Console.Commands.Input
{
    public class WithdrawInput
    {
        [Description("The number of the account")]
        public string AccountNumber { get; set; }
        
        [Description("The amount to withdraw, a positive number")]
        public decimal Amount { get; set; }

        [Description("The currency of the amount to withdraw, for example EUR")]
        public string Currency { get; set; }
    }
}