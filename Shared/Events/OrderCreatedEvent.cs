namespace Shared.Events;
public class OrderCreatedEvent
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public string ProductName { get; set; } = "";

    public decimal Price { get; set; }
}