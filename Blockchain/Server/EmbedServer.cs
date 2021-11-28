using Blockchain.Miner;
using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.WebApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Blockchain.Server
{
    public partial class EmbedServer : IRpcServer
    {
        private readonly IBlockMiner blockMiner;
        private readonly TransactionPool transactionPool;
        private readonly ILogger<EmbedServer> logger;

        private readonly WebServer server;
        private readonly string url;

        public EmbedServer(TransactionPool transactionPool, IBlockMiner blockMiner, ILoggerFactory loggerFactory, IConfigurationRoot config)
        {
            string port = config.GetSection("RpcPort").Get<string>();
            url = $"http://localhost:{port}/";

            server = CreateWebServer(url);
            this.transactionPool = transactionPool;
            this.blockMiner = blockMiner;
            logger = loggerFactory.CreateLogger<EmbedServer>();
        }

        public void Stop()
        {
            server.Dispose();
            logger.LogInformation("http server stopped");
        }

        public void Start()
        {
            // Once we've registered our modules and configured them, we call the RunAsync() method.
            server.RunAsync();
            logger.LogInformation($"http server available at {url}api");
        }

        private WebServer CreateWebServer(string url)
        {
            return new WebServer(webServerOptions => webServerOptions
                .WithUrlPrefix(url)
                .WithMode(HttpListenerMode.EmbedIO))
                .WithLocalSessionManager()
                .WithWebApi("/api", m => m.WithController(() => new Controller(blockMiner, transactionPool)))
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Error" }))
            );
        }
    }
}