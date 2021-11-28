﻿using System.Collections.Generic;
using System.Linq;
using Blockchain.Model;

namespace Blockchain.Miner
{
    public class TransactionPool
    {
        private readonly List<Transaction> rawTransactionList;

        private readonly object lockObj;

        public TransactionPool()
        {
            lockObj = new object();
            rawTransactionList = new List<Transaction>();
        }

        public void AddRaw(Transaction transaction)
        {
            lock (lockObj)
            {
                rawTransactionList.Add(transaction);
            }
        }
        public void AddRaw(string from, string to, int amount)
        {
            var transaction = new Transaction(from, to, amount);
            lock (lockObj)
            {
                rawTransactionList.Add(transaction);
            }
        }

        public List<Transaction> TakeAll()
        {
            lock (lockObj)
            {
                var all = rawTransactionList.ToList();
                rawTransactionList.Clear();
                return all;
            }
        }
    }
}
