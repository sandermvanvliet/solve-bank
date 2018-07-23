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
    public class WhenWithdrawingMoney
    {
        private readonly Mock<IBankAccountStore> _bankAccountStoreMock;
        private readonly Mock<IAccountAuthorisation> _accountAuthorisationMock;
        private readonly WithdrawUseCase _useCase;

        public WhenWithdrawingMoney()
        {
            _bankAccountStoreMock = new Mock<IBankAccountStore>();
            _accountAuthorisationMock = new Mock<IAccountAuthorisation>();
            _useCase = new WithdrawUseCase(_bankAccountStoreMock.Object, _accountAuthorisationMock.Object);

            GivenAuthorisationRequesSucceeds();
        }

        [Fact]
        public void GivenAccountDoesNotExist_AccountNotFoundExceptionIsThrown()
        {
            Action action = () =>  _useCase.WithdrawFrom("DOESNTEXIST", 10, "EUR");

            action
                .Should()
                .Throw<AccountNotFoundException>();
        }

        [Fact]
        public void GivenAmountIsLessThanZero_InvalidWithdrawalExceptionIsThrown()
        {
            var bankAccount = GivenBankAccountWithBalance(0);

            Action action = () =>  _useCase.WithdrawFrom(bankAccount.AccountNumber, -10, "EUR");

            action
                .Should()
                .Throw<InvalidWithDrawalException>();
        }

        [Fact]
        public void GivenBalanceIsZero_InsufficientBalanceExceptionIsThrown()
        {
            var bankAccount = GivenBankAccountWithBalance(0);

            Action action = () =>  _useCase.WithdrawFrom(bankAccount.AccountNumber, 10, "EUR");

            action
                .Should()
                .Throw<InsufficientBalanceException>();
        }

        [Fact]
        public void GivenBalanceIsTenAndWithdrawalOfTwenty_InsufficientBalanceExceptionIsThrown()
        {
            var bankAccount = GivenBankAccountWithBalance(10);

            Action action = () =>  _useCase.WithdrawFrom(bankAccount.AccountNumber, 20, "EUR");

            action
                .Should()
                .Throw<InsufficientBalanceException>();
        }

        [Fact]
        public void GivenBalanceIsTwentynAndWithdrawalOfTen_BalanceIsTen()
        {
            var bankAccount = GivenBankAccountWithBalance(20);

            _useCase.WithdrawFrom(bankAccount.AccountNumber, 10, "EUR");

            _bankAccountStoreMock
                .Object
                .GetByAccountNumber(bankAccount.AccountNumber)
                .Balance
                .Should()
                .Be(10);
        }

        [Fact]
        public void GivenAuthorisatioRequestnFails_AuthorisationFailedExceptionIsThrown()
        {
            var bankAccount = GivenBankAccountWithBalance(10);
            GivenAuthorisationRequestFails();

            Action action = () =>  _useCase.WithdrawFrom(bankAccount.AccountNumber, 10, "EUR");

            action
                .Should()
                .Throw<AuthorisationFailedException>();
        }

        private BankAccount GivenBankAccountWithBalance(decimal balance)
        {
            var bankAccount = new BankAccount
            {
                AccountNumber = "12345",
                Currency = "EUR"
            };

            _bankAccountStoreMock
                .Setup(b => b.GetByAccountNumber(It.Is<string>(s => s == bankAccount.AccountNumber)))
                .Returns(bankAccount);

            bankAccount.Deposit(balance, bankAccount.Currency);

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