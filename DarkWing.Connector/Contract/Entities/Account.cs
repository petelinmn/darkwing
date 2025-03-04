namespace DarkWing.Connector.Contract.Entities;

public class Account
{
    public AssetBalance[]? Balances { get; set; }
}

public class AssetBalance
{
    public string? Asset { get; set; }
    public decimal Free { get; set; }
    public decimal Locked { get; set; }
}
