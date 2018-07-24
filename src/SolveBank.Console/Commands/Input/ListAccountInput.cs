using System.ComponentModel;

namespace SolveBank.Console.Commands.Input
{
    public class ListAccountInput
    {
        [Description("The number of the account")]
        public string AccountNumber { get; set; }
    }
}