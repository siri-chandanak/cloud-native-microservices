using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using OrderService.DTOs;
using System.Security.Claims;

namespace OrderService.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!
                .Value);

        var orders = await _context.Orders
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(orders);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!
                .Value);

        var order = new Order
        {
            UserId = userId,
            ProductName = request.ProductName,
            Quantity = request.Quantity,
            Price = request.Price,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);

        await _context.SaveChangesAsync();

        return Ok(order);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!
                .Value);

        var order = await _context.Orders
            .FirstOrDefaultAsync(
                x => x.Id == id &&
                     x.UserId == userId);

        if (order == null)
        {
            return NotFound("Order not found.");
        }

        return Ok(order);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] CreateOrderRequest request)
    {
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!
                .Value);

        var order = await _context.Orders
            .FirstOrDefaultAsync(
                x => x.Id == id &&
                     x.UserId == userId);

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
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!
                .Value);

        var order = await _context.Orders
            .FirstOrDefaultAsync(
                x => x.Id == id &&
                     x.UserId == userId);

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
        var userId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!
                .Value);

        var order = await _context.Orders
            .FirstOrDefaultAsync(
                x => x.Id == id &&
                     x.UserId == userId);

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