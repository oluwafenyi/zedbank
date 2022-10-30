using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using zedbank.Core;
using zedbank.Models;
using zedbankInterestWorker.Contracts;

namespace zedbankInterestWorker.Consumers
{
    public class EverydayRecurringScheduleConsumer: IConsumer<EverydayRecurringScheduleElapsed>
    {
        private readonly ILogger<EverydayRecurringScheduleElapsed> _logger;

        public EverydayRecurringScheduleConsumer(ILogger<EverydayRecurringScheduleElapsed> logger)
        {
            _logger = logger;
        }
        
        public Task Consume(ConsumeContext<EverydayRecurringScheduleElapsed> context)
        {
            var midnightToday = Utils.GetStartOfDay(DateTimeOffset.Now);
            var aYearAgo = midnightToday.AddYears(-1);
            
            // get candidate wallets whose interests were applied over a year ago or exactly a year ago
            // select the last successful transaction for the previous day
            var wallets = Database.Context.Wallets
                .Where(w => w.LastInterestCredit <= aYearAgo || w.LastInterestCredit == null)
                .Include(w => w.Transactions.Where(t => t.Status == TransactionStatus.Successful && t.Created < midnightToday)
                    .OrderByDescending(t => t.Created)
                    .Take(1))
                .ToList();
            foreach (var wallet in wallets)
            {
                Transaction t;
                try
                {
                    t = wallet.Transactions.First();
                }
                catch (Exception e) when (e is InvalidOperationException or ArgumentNullException)
                {
                    continue;
                }
            
                var historicalBalance = t.HistoricalBalance;
                var finalBalance = t.Type == TransactionType.Credit  // sum to get closing balance for the day
                    ? historicalBalance + t.Amount
                    : historicalBalance - t.Amount;
                if (finalBalance == 0)
                {
                    continue;
                }
                var walletId = wallet.Id;
                
                // publish message so consumer can calculate interest and credit wallet
                context.Publish<ZedbankInterestWorker>(new()
                {
                    WalletId = walletId,
                    Balance = finalBalance,
                });
            }
            _logger.LogInformation("Interest Task Distribution Complete");
            return Task.CompletedTask;
        }
    }
}