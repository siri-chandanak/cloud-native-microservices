namespace OrderService.DTOs;
public class CreateOrderRequest
{
    public string ProductName { get; set; } = "";

    public int Quantity { get; set; }

    public decimal Price { get; set; }
}