using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blockchain.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Blockchain.Miner
{
    public class BlockMiner : IBlockMiner
    {
        private static readonly int MINING_PERIOD = 10000;
        private static readonly int MINING_REWARD = 2;

        private readonly TransactionPool transactionPool;
        private readonly ILogger<BlockMiner> logger;

        private readonly string nodeName;

        public List<Block> Blockchain { get; private set; }
        private CancellationTokenSource cancellationToken;

        public BlockMiner(TransactionPool transactionPool, ILoggerFactory loggerFactory, IConfigurationRoot config)
        {
            Blockchain = new List<Block>();
            this.transactionPool = transactionPool;
            logger = loggerFactory.CreateLogger<BlockMiner>();
            nodeName = config.GetSection("NodeName").Get<string>();
        }

        public void Start()
        {
            cancellationToken = new CancellationTokenSource();
            Task.Run(() => DoGenerateBlock(), cancellationToken.Token);
            logger.LogInformation("Mining has started");
        }

        public void Stop()
        {
            cancellationToken.Cancel();
            logger.LogInformation("Mining has stopped");
        }

        private void DoGenerateBlock()
        {
            while (true)
            {
                var startTime = DateTime.Now.Millisecond;
                GenerateBlock();

                var endTime = DateTime.Now.Millisecond;
                var remainingTime = MINING_PERIOD - (endTime - startTime);

                Thread.Sleep(remainingTime < 0 ? 0 : remainingTime);
            }
        }

        private void GenerateBlock()
        {
            var lastBlock = Blockchain.LastOrDefault();
            var transactionList = transactionPool.TakeAll();
            transactionList.Add(new Transaction()
            {
                Amount = MINING_REWARD,
                From = "-",
                To = nodeName
            });

            var block = new Block
            {
                TimeStamp = DateTime.Now,
                Nonce = 0,
                TransactionList = transactionList,
                Index = lastBlock?.Index + 1 ?? 0,
                PrevHash = lastBlock?.Hash ?? string.Empty
            };

            MineBlock(block);
            Blockchain.Add(block);

            logger.LogInformation("GenerateBlock");
        }

        private void MineBlock(Block block)
        {
            var merkleRootHash = FindMerkleRootHash(block.TransactionList);
            long nonce = -1;
            string hash;
            do
            {
                nonce++;
                var rawData = block.Index + block.PrevHash + block.TimeStamp.ToString() + nonce + merkleRootHash;
                hash = CalculateHash(CalculateHash(rawData));
            }
            while (!hash.StartsWith("0000"));

            block.Hash = hash;
            block.Nonce = nonce;
        }

        private string FindMerkleRootHash(IList<Transaction> transactionList)
        {
            var transactionStrList = transactionList.Select(tran => CalculateHash(CalculateHash(tran.From + tran.To + tran.Amount))).ToList();
            return BuildMerkleRootHash(transactionStrList);
        }

        private string BuildMerkleRootHash(IList<string> merkelLeaves)
        {
            if (merkelLeaves == null || !merkelLeaves.Any())
            {
                return string.Empty;
            }

            if (merkelLeaves.Count == 1)
            {
                return merkelLeaves.First();
            }

            if (merkelLeaves.Count % 2 > 0)
            {
                merkelLeaves.Add(merkelLeaves.Last());
            }

            var merkleBranches = new List<string>();

            for (int i = 0; i < merkelLeaves.Count; i += 2)
            {
                var leafPair = string.Concat(merkelLeaves[i], merkelLeaves[i + 1]);
                merkleBranches.Add(CalculateHash(CalculateHash(leafPair)));
            }

            return BuildMerkleRootHash(merkleBranches);
        }

        private static string CalculateHash(string rawData)
        {
            // Create a SHA256   
            using SHA256 sha256Hash = SHA256.Create();

            // ComputeHash - returns byte array  
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            // Convert byte array to a string   
            var builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }
}