using System.Net;
using FluentAssertions;
using Moq;
using SolveBank.Console.Commands;
using SolveBank.Console.Commands.Input;
using SolveBank.Models;
using SolveBank.Ports.Authorisation;
using SolveBank.Ports.Persistence;
using Xunit;

namespace SolveBank.Console.Tests.Integration.Commands
{
    public class WhenDepositingMoney
    {
        private readonly DepositCommand _command;
        private readonly Mock<IAccountAuthorisation> _accountAuthorisationMock;
        private readonly IBankAccountStore _bankAccountStore;

        public WhenDepositingMoney()
        {
            _accountAuthorisationMock = new Mock<IAccountAuthorisation>();

            var container = Bootstrapper.BootstrapContainerAndAdapters(c =>
                {
                    c.RegisterInstance(_accountAuthorisationMock.Object);
                });

            _command = container.GetInstance<DepositCommand>();
            _bankAccountStore = container.GetInstance<IBankAccountStore>();
        }

        [Fact]
        public void GivenTheAccountDoesNotExist_CommandReturnsFalse()
        {
            var result = _command.Execute(new DepositInput
            {
                AccountNumber = "DOESNTEXIST",
                Amount = 10,
                Currency = "EUR"
            });

            result
                .Should()
                .BeFalse();
        }

        [Fact]
        public void GivenTheAuthorisationFails_CommandReturnsFalse()
        {
            var bankAccount = GivenAnAccount();

            var result = _command.Execute(new DepositInput
            {
                AccountNumber = bankAccount.AccountNumber,
                Amount = 10,
                Currency = "EUR"
            });

            result
                .Should()
                .BeFalse();
        }
        
        [Fact]
        public void GivenTheAuthorisationSucceeds_CommandReturnsTrue()
        {
            var bankAccount = GivenAnAccount();

            _accountAuthorisationMock
                .Setup(a => a.RequestForDeposit(It.IsAny<BankAccount>()))
                .Returns(true);

            var result = _command.Execute(new DepositInput
            {
                AccountNumber = bankAccount.AccountNumber,
                Amount = 10,
                Currency = "EUR"
            });

            result
                .Should()
                .BeTrue();
        }

        private BankAccount GivenAnAccount()
        {
            var bankAccount = new BankAccount
            {
                AccountNumber = "12345",
                Currency = "EUR"
            };

            _bankAccountStore.Store(bankAccount);

            return bankAccount;
        }
    }
}