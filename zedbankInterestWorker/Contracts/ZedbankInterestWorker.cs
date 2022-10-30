namespace zedbankInterestWorker.Contracts
{
    public record ZedbankInterestWorker
    {
        public decimal Balance { get; init; }
        public long WalletId { get; init; }
    }
}