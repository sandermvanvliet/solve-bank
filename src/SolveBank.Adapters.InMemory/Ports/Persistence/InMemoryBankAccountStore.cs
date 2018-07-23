﻿using System.Collections.Generic;
using Newtonsoft.Json;
using SolveBank.Models;
using SolveBank.Ports.Persistence;

namespace SolveBank.Adapters.InMemory.Ports.Persistence
{
    internal class InMemoryBankAccountStore : IBankAccountStore
    {
        private readonly Dictionary<string, BankAccount> _bankAccounts;

        public InMemoryBankAccountStore()
        {
            _bankAccounts = new Dictionary<string, BankAccount>();
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
    }
}