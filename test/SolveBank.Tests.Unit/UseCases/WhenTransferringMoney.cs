using System;
using FluentAssertions;
using Moq;
using SolveBank.Exceptions;
using SolveBank.Models;
using SolveBank.Ports.Authorisation;
using SolveBank.Ports.Persistence;
using SolveBank.UseCases;
using Xunit;

namespace SolveBank.Tests.Unit.UseCases
{
    public class WhenTransferringMoney
    {
        
        private readonly Mock<IBankAccountStore> _bankAccountStoreMock;
        private readonly Mock<IAccountAuthorisation> _accountAuthorisationMock;
        private readonly TransferUseCase _useCase;
        private const string SourceAccountNumber = "1234";
        private const string DestinationAccountNumber = "5678";

        public WhenTransferringMoney()
        {
            _bankAccountStoreMock = new Mock<IBankAccountStore>();
            _accountAuthorisationMock = new Mock<IAccountAuthorisation>();
            _useCase = new TransferUseCase(_bankAccountStoreMock.Object, _accountAuthorisationMock.Object);

            GivenAuthorisationRequesSucceeds();
        }

        [Fact]
        public void GivenSourceAccountDoesNotExist_AccountNotFoundExceptionIsThrown()
        {
            Action action = () => _useCase.Transfer(
                10,
                "EUR",
                SourceAccountNumber,
                DestinationAccountNumber);

            action.Should()
                .Throw<AccountNotFoundException>();
        }

        [Fact]
        public void GivenDestinationAccountDoesNotExist_AccountNotFoundExceptionIsThrown()
        {
            GivenBankAccountWithBalance(SourceAccountNumber, 10);

            Action action = () => _useCase.Transfer(
                10,
                "EUR",
                SourceAccountNumber,
                DestinationAccountNumber);

            action.Should()
                .Throw<AccountNotFoundException>();
        }

        [Fact]
        public void GivenAmountIsLessThanZero_InvalidWithdrawalExceptionIsThrown()
        {
            GivenBankAccountWithBalance(SourceAccountNumber, 10);
            GivenBankAccountWithBalance(DestinationAccountNumber, 0);

            Action action = () =>  _useCase.Transfer(
                -20,
                "EUR",
                SourceAccountNumber,
                DestinationAccountNumber);

            action
                .Should()
                .Throw<InvalidTransferException>();
        }

        [Fact]
        public void GivenSourceAccountBalanceInsufficient_InsufficientBalanceExceptionIsThrown()
        {
            GivenBankAccountWithBalance(SourceAccountNumber, 10);
            GivenBankAccountWithBalance(DestinationAccountNumber, 0);

            Action action = () => _useCase.Transfer(
                20,
                "EUR",
                SourceAccountNumber,
                DestinationAccountNumber);

            action.Should()
                .Throw<InsufficientBalanceException>();
        }

        [Fact]
        public void GivenTenEurosTransferred_SourceAccountBalanceIsTenEurosLess()
        {
            var sourceAccount = GivenBankAccountWithBalance(SourceAccountNumber, 10);
            GivenBankAccountWithBalance(DestinationAccountNumber, 0);

            _useCase.Transfer(
                10,
                "EUR",
                SourceAccountNumber,
                DestinationAccountNumber);

            sourceAccount
                .Balance
                .Should()
                .Be(0);
        }

        [Fact]
        public void GivenTenEurosTransferred_DestinationAccountBalanceIsTenEurosMore()
        {
            GivenBankAccountWithBalance(SourceAccountNumber, 10);
            var destinationAccount = GivenBankAccountWithBalance(DestinationAccountNumber, 0);

            _useCase.Transfer(
                10,
                "EUR",
                SourceAccountNumber,
                DestinationAccountNumber);

            destinationAccount
                .Balance
                .Should()
                .Be(10);
        }

        [Fact]
        public void GivenSourceAccountAuthorisationFails_AuthorisationFailedExceptionIsThrown()
        {
            GivenBankAccountWithBalance(SourceAccountNumber, 10);
            GivenBankAccountWithBalance(DestinationAccountNumber, 0);

            GivenAuthorisationRequestFails();

            Action action = () => _useCase.Transfer(
                10,
                "EUR",
                SourceAccountNumber,
                DestinationAccountNumber);

            action
                .Should()
                .Throw<AuthorisationFailedException>();
        }

        private BankAccount GivenBankAccountWithBalance(string accountNumber, decimal balance)
        {
            
            var bankAccount = new BankAccount
            {
                AccountNumber = accountNumber,
                Currency = "EUR"
            };

            _bankAccountStoreMock
                .Setup(b => b.GetByAccountNumber(It.Is<string>(s => s == bankAccount.AccountNumber)))
                .Returns(bankAccount);

            if(balance > 0)
            {
                bankAccount.Deposit(balance, bankAccount.Currency);
            }

            return bankAccount;
        }

        private void GivenAuthorisationRequesSucceeds()
        {
            _accountAuthorisationMock
                .Setup(a => a.RequestForWithdrawal(It.IsAny<BankAccount>()))
                .Returns(true);
        }

        private void GivenAuthorisationRequestFails()
        {
            _accountAuthorisationMock
                .Setup(a => a.RequestForWithdrawal(It.IsAny<BankAccount>()))
                .Returns(false);
        }
    }
}