namespace DarkWing.Connector.Contract.Entities;

public class CreatedOrder
{
    public string? Symbol { get; set; }
    public int OrderId { get; set; }
    public string? ClientOrderId { get; set; }
}
