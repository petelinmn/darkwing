namespace DarkWing.Connector;

public class PublicExchangeCredential
{
    public Exchange Exchange { get; set; }
    public string? BaseUrl { get; set; }
    public string? OriginExchangeBaseUrl { get; set; }
}

public class ExchangeCredential : PublicExchangeCredential
{
    public string? Title { get; set; }
    public string? Key { get; set; }
    public string? Secret { get; set; }
}
