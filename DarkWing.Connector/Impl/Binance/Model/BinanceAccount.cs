namespace DarkWing.Connector.Impl.Binance.Model;

public class BinanceAccount
{
    public long Uid { get; set; }
    public decimal MakerComission { get; set; }
    public decimal TakerComission { get; set; }
    public decimal BuyerCommission { get; set; }
    public decimal SellerCommission { get; set; }
    public bool CanTrade { get; set; }
    public bool CanWithdraw { get; set; }
    public bool CanDeposit { get; set; }
    public bool Brokered { get; set; }
    public bool RequireSelfTradePrevention { get; set; }
    public bool PreventSor { get; set; }
    public long UpdateTime { get; set; }
    public string? AccountType { get; set; }
    public BinanceComissionRate[]? ComissionRates { get; set; }
    public BinanceAssetBalance[]? Balances { get; set; }
}

public class BinanceAssetBalance
{
    public string? Asset { get; set; }
    public decimal Free { get; set; }
    public decimal Locked { get; set; }
}

public class BinanceComissionRate
{
    public decimal Maker { get; set; }
    public decimal Taker { get; set; }
    public decimal Buyer { get; set; }
    public decimal Seller { get; set; }
}
