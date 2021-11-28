using System.Collections.Generic;
using System.Linq;
using Blockchain.Miner;
using Blockchain.Model;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace Blockchain.Server
{
    public sealed class Controller : WebApiController
    {
        private readonly IBlockMiner blockMiner;
        private readonly TransactionPool transactionPool;

        public Controller(IBlockMiner blockMiner, TransactionPool transactionPool)
        {
            this.blockMiner = blockMiner;
            this.transactionPool = transactionPool;
        }

        //GET http://localhost:9696/api/blocks
        [Route(HttpVerbs.Get, "/blocks")]
        public List<Block> GetAllBlocks() => blockMiner.Blockchain;

        //GET http://localhost:9696/api/blocks/index/{index?}
        [Route(HttpVerbs.Get, "/blocks/index/{index?}")]
        public Block GetAllBlocks(int index)
        {
            Model.Block block = null;
            if (index < blockMiner.Blockchain.Count)
            {
                block = blockMiner.Blockchain[index];
            }
            return block; // JsonSerializer.Serialize(block);
        }

        //GET http://localhost:9696/api/blocks/latest
        [Route(HttpVerbs.Get, "/blocks/latest")]
        public Block GetLatestBlocks()
        {
            var block = blockMiner.Blockchain.LastOrDefault();
            return block; // JsonSerializer.Serialize(block);
        }

        //Post http://localhost:9696/api/add
        //Body >> {"From":"amir","To":"bob","Amount":10}
        [Route(HttpVerbs.Post, "/add")]
        public void AddTransaction()
        {
            var data = HttpContext.GetRequestDataAsync<Transaction>();
            if (data != null && data.Result != null)
            {
                transactionPool.AddRaw(data.Result);
            }
        }
    }
}