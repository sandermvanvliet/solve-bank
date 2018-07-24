using System;
using Oakton;
using SolveBank.Console.Commands.Input;
using SolveBank.Exceptions;
using SolveBank.UseCases;

namespace SolveBank.Console.Commands
{
    [Description("Retrieve details of bank account", Name = "list-account")]
    public class ListAccountCommand : OaktonCommand<ListAccountInput>
    {
        private readonly RetrieveBankAccountUseCase _useCase;

        public ListAccountCommand(RetrieveBankAccountUseCase useCase)
        {
            _useCase = useCase;
        }

        public override bool Execute(ListAccountInput input)
        {
            if (string.IsNullOrEmpty(input.AccountNumber))
            {
                throw new ArgumentNullException(nameof(input.AccountNumber));
            }

            try
            {
                var bankAccount = _useCase.GetByAccountNumber(input.AccountNumber);

                System.Console.WriteLine($"Bank account: {bankAccount.Customer.Name}");
            }
            catch (AccountNotFoundException)
            {
                System.Console.Error.WriteLine("Account number does not exist as a checking account");
                return false;
            }

            return true;
        }
    }
}