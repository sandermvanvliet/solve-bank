﻿using System.Collections.Generic;
using Newtonsoft.Json;
using SolveBank.Models;
using SolveBank.Ports.Persistence;

namespace SolveBank.Adapters.Persistence.Ports.Persistence
{
    internal class InMemoryBankAccountStore : IBankAccountStore
    {
        private readonly Dictionary<string, BankAccount> _bankAccounts;

        public InMemoryBankAccountStore()
        {
            _bankAccounts = new Dictionary<string, BankAccount>();

            Store(new BankAccount
            {
                AccountNumber = "12345",
                Currency = "EUR",
                Customer = new Customer
                {
                    Name = "Joe Blogs",
                    SecurityQuestion = "What is your favourite colour",
                    SecurityAnswer = "Blue"
                }
            });
            
            Store(new BankAccount
            {
                AccountNumber = "67890",
                Currency = "EUR",
                Customer = new Customer
                {
                    Name = "Bob Blogs",
                    SecurityQuestion = "What is your favourite colour",
                    SecurityAnswer = "Red"
                }
            });
        }

        public BankAccount GetByAccountNumber(string accountNumber)
        {
            if(!_bankAccounts.ContainsKey(accountNumber))
            {
                return null;
            }

            var account = _bankAccounts[accountNumber];

            var serialized = JsonConvert.SerializeObject(account);

            return JsonConvert.DeserializeObject<BankAccount>(serialized);
        }

        public void Store(BankAccount bankAccount)
        {
            var serialized = JsonConvert.SerializeObject(bankAccount);
            
            var clonedBankAccount = JsonConvert.DeserializeObject<BankAccount>(serialized);

            if (!_bankAccounts.ContainsKey(bankAccount.AccountNumber))
            {
                _bankAccounts.Add(bankAccount.AccountNumber, clonedBankAccount);
            }
            else
            {
                _bankAccounts[bankAccount.AccountNumber] = clonedBankAccount;
            }
        }
    }
}