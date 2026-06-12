namespace OrderService.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string ProductName { get; set; } = "";

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}