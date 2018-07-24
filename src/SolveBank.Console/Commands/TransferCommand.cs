using Oakton;
using SolveBank.Console.Commands.Input;
using SolveBank.Exceptions;
using SolveBank.UseCases;

namespace SolveBank.Console.Commands
{
    [Description("Transfer money from one account to another", Name = "transfer")]
    public class TransferCommand : OaktonCommand<TransferInput>
    {
        private readonly TransferUseCase _useCase;

        public TransferCommand(TransferUseCase useCase)
        {
            _useCase = useCase;
        }

        public override bool Execute(TransferInput input)
        {
            try
            {
                _useCase.Transfer(
                    input.Amount,
                    input.Currency,
                    input.SourceAccountNumber,
                    input.DestinationAccountNumber);
            }
            catch (AccountNotFoundException)
            {
                System.Console.Error.WriteLine("Failed to transfer: Account number does not exist as a checking account");
                return false;
            }
            catch (AuthorisationFailedException)
            {
                System.Console.Error.WriteLine("Failed to transfer: Authorisation failed");
                return false;
            }
            catch (InsufficientBalanceException ibx)
            {
                System.Console.Error.WriteLine($"Failed to transfer: {ibx.Message}");
                return false;
            }

            return true;
        }
    }
}
