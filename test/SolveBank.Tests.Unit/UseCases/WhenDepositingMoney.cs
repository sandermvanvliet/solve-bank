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
    public class WhenDepositingMoney
    {
        private readonly Mock<IBankAccountStore> _bankAccountStoreMock;
        private readonly Mock<IAccountAuthorisation> _accountAuthorisationMock;
        private readonly DepositUseCase _useCase;

        public WhenDepositingMoney()
        {
            _bankAccountStoreMock = new Mock<IBankAccountStore>();
            _accountAuthorisationMock = new Mock<IAccountAuthorisation>();
            _useCase = new DepositUseCase(_bankAccountStoreMock.Object, _accountAuthorisationMock.Object);

            GivenAuthorisationRequesSucceeds();
        }

        [Fact]
        public void GivenAccountDoesNotExist_AccountNotFoundExceptionIsThrown()
        {
            Action action = () =>  _useCase.DepositTo("DOESNTEXIST", 10, "EUR");

            action
                .Should()
                .Throw<AccountNotFoundException>();
        }

        [Fact]
        public void GivenADepositOfTenEuros_BankBalanceIsTenEuros()
        {
            var bankAccount = GivenANewEmptyBankAccount();

            _useCase.DepositTo(bankAccount.AccountNumber, 10, "EUR");

            bankAccount
                .Balance
                .Should()
                .Be(10);
        }

        [Fact]
        public void GivenADepositOfMinusTenEuros_InvalidDepositExceptionIsThrown()
        {
            var bankAccount = GivenANewEmptyBankAccount();

            Action action = () => _useCase.DepositTo(bankAccount.AccountNumber, -10, "EUR");

            action
                .Should()
                .Throw<InvalidDepositException>();
        }

        [Fact]
        public void GivenAuthorisationRequestFails_AuthorisationFailedExceptionIsThrown()
        {
            GivenAuthorisationRequestFails();
            var bankAccount = GivenANewEmptyBankAccount();

            Action action = () => _useCase.DepositTo(bankAccount.AccountNumber, 10, "EUR");

            action
                .Should()
                .Throw<AuthorisationFailedException>();
        }

        private BankAccount GivenANewEmptyBankAccount()
        {
            var bankAccount = new BankAccount
            {
                AccountNumber = "12345",
                Currency = "EUR"
            };

            _bankAccountStoreMock
                .Setup(b => b.GetByAccountNumber(It.Is<string>(s => s == bankAccount.AccountNumber)))
                .Returns(bankAccount);

            return bankAccount;
        }

        private void GivenAuthorisationRequesSucceeds()
        {
            _accountAuthorisationMock
                .Setup(a => a.RequestForDeposit(It.IsAny<BankAccount>()))
                .Returns(true);
        }

        private void GivenAuthorisationRequestFails()
        {
            _accountAuthorisationMock
                .Setup(a => a.RequestForDeposit(It.IsAny<BankAccount>()))
                .Returns(false);
        }
    }
}