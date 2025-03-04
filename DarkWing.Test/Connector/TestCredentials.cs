using DarkWing.Connector;

namespace DarkWing.Test.Connector;

public static class TestCredentials
{
    public static ExchangeCredential BinanceCredential { get; } = new ExchangeCredential()
    {
        Exchange = Exchange.Binance,
        BaseUrl = "https://api.binance.com",
        Title = "Binance",
        Key = "bYg60g4mjNLpvltTA1FcYsmZCmLZMqPZinAfccthDsEW2FRjWM88wq4NXA7KG0st",
        Secret = "Idmx7h9avHSJ8BgvCoLzeYEFTDLvh1LQf8Y6AYeoIIlIcowqHkM5ZOYm197ehN4o"
    };

    public static ExchangeCredential BinanceTestCredential { get; } = new()
    {
        Exchange = Exchange.BinanceTest,
        BaseUrl = "https://testnet.binance.vision",
        OriginExchangeBaseUrl = "https://api.binance.com",
        Title = "BinanceTest",
        Key = "9qlOxFOKC6u0MofpiEIWurZSnaRv0Aqo72oOUA9M5KL0IHD3A0HpF9nVkehQDmzi",
        Secret = "PSr4aTtUb4q2bdjLHK0XSj900mMbY38yrl3nHHO6nsciyWaOhytjtVB0cBAC3g94"
    };

    public static ExchangeCredential ByBitCredential { get; } = new()
    {
        Exchange = Exchange.ByBit,
        BaseUrl = "https://api.bybit.com",
        Title = "ByBit",
        Key = "B0PcrUaDVfYJnWlHU3",
        Secret = "A1Iqq9bLkd3pD4DGxOCpJIUjyEMwu2CGeY5M"
    };

    public static ExchangeCredential ByBitTestCredential { get; } = new()
    {
        Exchange = Exchange.ByBit,
        BaseUrl = "https://api-testnet.bybit.com",
        Title = "ByBitTest",
        Key = "u5ZrCon5jkPY3G86np",
        Secret = "tO2Pnk6Tj0ao49GM64nIDirzQEe83OYJg212"
    };

    public static ExchangeCredential[] CredentialsToAllTest { get; } = new[] {
        BinanceCredential,
        BinanceTestCredential,
        ByBitCredential,
        ByBitTestCredential
    };

    public static ExchangeCredential[] CredentialsToSensitiveTest { get; } = new[] {
        BinanceTestCredential,
        ByBitTestCredential
    };
}