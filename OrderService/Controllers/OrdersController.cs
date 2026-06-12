using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc; 
using Microsoft.EntityFrameworkCore; 
using OrderService.Data; 
using OrderService.DTOs; 
using OrderService.Models; 
using OrderService.Services; 
using Shared.Events; 
using System.Security.Claims;

namespace OrderService.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    private readonly RabbitMqPublisher _publisher;

    private int CurrentUserId =>
        int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!
                .Value);

    public OrdersController(
        ApplicationDbContext context,
        RabbitMqPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _context.Orders 
            .Where(x => x.UserId == CurrentUserId) 
            .OrderByDescending(x => x.CreatedAt) 
            .ToListAsync();

        return Ok(orders);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request)
    {
        var order = new Order
        {
            UserId = CurrentUserId,
            ProductName = request.ProductName,
            Quantity = request.Quantity,
            Price = request.Price,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);

        await _context.SaveChangesAsync();

        try
        {
            var orderCreatedEvent =
                new OrderCreatedEvent
                {
                    OrderId = order.Id,
                    UserId = order.UserId,
                    ProductName = order.ProductName,
                    Price = order.Price
                };

            _publisher.Publish(
                "order-created",
                orderCreatedEvent);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"RabbitMQ publish failed: {ex.Message}");
        }

        return Ok(order);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(
                x => x.Id == id &&
                     x.UserId == CurrentUserId);

        if (order == null)
        {
            return NotFound("Order not found.");
        }

        return Ok(order);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] CreateOrderRequest request)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(
                x => x.Id == id &&
                     x.UserId == CurrentUserId);

        if (order == null)
        {
            return NotFound("Order not found.");
        }

        order.ProductName = request.ProductName;
        order.Quantity = request.Quantity;
        order.Price = request.Price;

        await _context.SaveChangesAsync();

        return Ok(order);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(
                x => x.Id == id &&
                     x.UserId == CurrentUserId);

        if (order == null)
        {
            return NotFound("Order not found.");
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Order deleted successfully"
        });
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(
                x => x.Id == id &&
                     x.UserId == CurrentUserId);

        if (order == null)
        {
            return NotFound("Order not found.");
        }

        var allowedStatuses = new[]
        {
            "Pending",
            "Processing",
            "Shipped",
            "Delivered",
            "Cancelled"
        };

        if (!allowedStatuses.Contains(
                request.Status,
                StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(
                $"Invalid status. Allowed values: {string.Join(", ", allowedStatuses)}");
        }

        order.Status = request.Status;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Order status updated successfully",
            OrderId = order.Id,
            Status = order.Status
        });
    }
}