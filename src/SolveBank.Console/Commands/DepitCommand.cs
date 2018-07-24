using Oakton;
using SolveBank.Console.Commands.Input;
using SolveBank.Exceptions;
using SolveBank.UseCases;

namespace SolveBank.Console.Commands
{
    [Description("Deposit money to an account", Name ="deposit")]
    public class DepositCommand : OaktonCommand<DepositInput>
    {
        private readonly DepositUseCase _useCase;

        public DepositCommand(DepositUseCase useCase)
        {
            _useCase = useCase;
        }

        public override bool Execute(DepositInput input)
        {
            try
            {
                _useCase.DepositTo(input.AccountNumber,
                    input.Amount,
                    input.Currency);
            }
            catch (AccountNotFoundException)
            {
                System.Console.Error.WriteLine("Failed to withdraw: Account number does not exist as a checking account");
                return false;
            }
            catch (AuthorisationFailedException)
            {
                System.Console.Error.WriteLine("Failed to withdraw: Authorisation failed");
                return false;
            }

            return true;
        }
    }
}