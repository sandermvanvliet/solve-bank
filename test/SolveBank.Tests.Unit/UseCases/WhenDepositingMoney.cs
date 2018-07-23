using System;
using FluentAssertions;
using Moq;
using SolveBank.Exceptions;
using SolveBank.Models;
using SolveBank.Ports.Persistence;
using SolveBank.UseCases;
using Xunit;

namespace SolveBank.Tests.Unit.UseCases
{
    public class WhenDepositingMoney
    {
        private readonly Mock<IBankAccountStore> _bankAccountStoreMock;
        private readonly DepositUseCase _useCase;

        public WhenDepositingMoney()
        {
            _bankAccountStoreMock = new Mock<IBankAccountStore>();
            _useCase = new DepositUseCase(_bankAccountStoreMock.Object);
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
    }
}