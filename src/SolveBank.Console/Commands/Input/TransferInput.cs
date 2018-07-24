using Oakton;

namespace SolveBank.Console.Commands.Input
{
    public class TransferInput
    {
        [Description("The amount to transfer, a positive number")]
        public decimal Amount { get; set; }

        [Description("The currency of the amount to transfer, for example EUR")]
        public string Currency { get; set; }

        [Description("The number of the account to transfer money from")]
        public string SourceAccountNumber { get; set; }
        
        [Description("The number of the account to transfer money to")]
        public string DestinationAccountNumber { get; set; }
    }
}