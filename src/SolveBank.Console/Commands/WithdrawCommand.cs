using Oakton;
using SolveBank.Console.Commands.Input;
using SolveBank.Exceptions;
using SolveBank.UseCases;

namespace SolveBank.Console.Commands
{
    [Description("Withdraw money from an account", Name ="withdraw")]
    public class WithdrawCommand : OaktonCommand<WithdrawInput>
    {
        private readonly WithdrawUseCase _useCase;

        public WithdrawCommand(WithdrawUseCase useCase)
        {
            _useCase = useCase;
        }

        public override bool Execute(WithdrawInput input)
        {
            try
            {
                _useCase.WithdrawFrom(input.AccountNumber,
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
            catch (InsufficientBalanceException ibx)
            {
                System.Console.Error.WriteLine($"Failed to withdraw: {ibx.Message}");
                return false;
            }

            return true;
        }
    }
}