using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using zedbank.Core;
using zedbank.Models;
using zedbank.Services;
using zedbankInterestWorker.Contracts;

namespace zedbankInterestWorker.Consumers
{
    public class ZedbankInterestWorkerConsumer :
        IConsumer<ZedbankInterestWorker>
    {
        private readonly ILogger<ZedbankInterestWorkerConsumer> _logger;
        private decimal _interestRate = 0.10m;

        public ZedbankInterestWorkerConsumer(ILogger<ZedbankInterestWorkerConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ZedbankInterestWorker> context)
        {
            _logger.LogInformation("Received Interest Task");
            var interestAmount = context.Message.Balance * _interestRate;
            var wallet = await Database.Context.Wallets.FindAsync(context.Message.WalletId);
            if (wallet == null)
            {
                _logger.LogWarning("Could not find the wallet with Id: {wId}", context.Message.WalletId);
                return;
            }
            
            await WalletService.FundWallet(wallet, new WalletTransactionDto(interestAmount.ToString("0.00")),
                TransactionClassification.SimpleInterestCredit, Database.Context);
            _logger.LogInformation("Successfully applied interest to wallet with Id: {wId}", context.Message.WalletId);
        }
    }
}