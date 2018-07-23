using System;
using FluentAssertions;
using Moq;
using SolveBank.Exceptions;
using SolveBank.Ports.Persistence;
using SolveBank.UseCases;
using Xunit;

namespace SolveBank.Tests.Unit.UseCases
{
    public class WhenRetrievingABankAccount
    {
        private readonly Mock<IBankAccountStore> _bankAccountStoreMock;
        private readonly RetrieveBankAccountUseCase _useCase;

        public WhenRetrievingABankAccount()
        {
            _bankAccountStoreMock = new Mock<IBankAccountStore>();
            _useCase = new RetrieveBankAccountUseCase(_bankAccountStoreMock.Object);
        }

        [Fact]
        public void GivenAnEmptyAccountNumber_ArgumentNullExceptionIsThrown()
        {
            Action action = () => _useCase.GetByAccountNumber(null);

            action
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public void GivenAccountNumberThatDoesntExist_AccountNotFoundExceptionIsThrown()
        {
            Action action = () => _useCase.GetByAccountNumber("123456");

            action
                .Should()
                .Throw<AccountNotFoundException>();
        }

        [Fact]
        public void GivenBankAccountExists_BankAccountIsReturned()
        {
            _bankAccountStoreMock
                .Setup(b => b.GetByAccountNumber(It.Is<string>(s => s == "123456")))
                .Returns(new Models.BankAccount { AccountNumber = "123456", Currency = "EUR" });

            var bankAccount = _useCase.GetByAccountNumber("123456");

            bankAccount
                .AccountNumber
                .Should()
                .Be("123456");
        }
    }
}