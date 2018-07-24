using FluentAssertions;
using Moq;
using SolveBank.Adapters.InMemory.Ports.Persistence;
using SolveBank.Models;
using SolveBank.Ports.Authorisation;
using SolveBank.Ports.Persistence;
using Xunit;

namespace SolveBank.Console.Tests.Integration
{
    public class WhenDepositingMoneyThrougCli
    {
        private Program _program;
        private readonly IBankAccountStore _bankAccountStore;

        public WhenDepositingMoneyThrougCli()
        {
            var accountAuthorisationMock = new Mock<IAccountAuthorisation>();
            
            accountAuthorisationMock
                .Setup(a => a.RequestForDeposit(It.IsAny<BankAccount>()))
                .Returns(true);

            _bankAccountStore = new InMemoryBankAccountStore();

            _program = new Program(c =>
            {
                c.RegisterInstance(accountAuthorisationMock.Object);
                c.RegisterInstance(_bankAccountStore);
            });
        }

        [Fact]
        public void GivenArgumentsToDepositMoneyAnDepositSucceeds_ExistCodeIsZero()
        {
            var bankAccount = GivenAnAccount();

            var exitCode = _program.Run(new[]
            {
                "deposit",
                bankAccount.AccountNumber,
                "10",
                "EUR"
            });

            exitCode
                .Should()
                .Be(0);
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