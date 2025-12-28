using MaghsalatiSPlus.Data;
using MaghsalatiSPlus.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaghsalatiSPlus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .ToListAsync();
        }

 
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return Ok(order);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Order>> PostOrder([FromForm] CreateOrderDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var customer = await _context.Customers.FindAsync(dto.CustomerId);
            if (customer == null) return BadRequest("Customer not found.");

            if (string.IsNullOrWhiteSpace(dto.OrderItemsJson))
                return BadRequest("OrderItemsJson is required.");

            List<CreateOrderItemDto>? orderItemsList;
            try
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                jsonOptions.Converters.Add(new JsonStringEnumConverter());

              
                var raw = dto.OrderItemsJson.Trim();
                if (raw.StartsWith("{") && raw.EndsWith("}"))
                {
                    raw = "[" + raw + "]";
                }

                orderItemsList = JsonSerializer.Deserialize<List<CreateOrderItemDto>>(raw, jsonOptions);

                if (orderItemsList == null || !orderItemsList.Any())
                    return BadRequest("No valid items parsed from OrderItemsJson.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error parsing OrderItemsJson: {ex.Message}");
            }

            foreach (var item in orderItemsList)
            {
                if (string.IsNullOrWhiteSpace(item.ItemName))
                    return BadRequest("Each item must have a valid ItemName.");
                if (item.Quantity <= 0)
                    return BadRequest("Quantity must be greater than 0.");
                if (item.Price < 0)
                    return BadRequest("Price must be non-negative.");
                if (!await _context.Categories.AnyAsync(c => c.Id == item.CategoryId))
                    return BadRequest($"Category with ID {item.CategoryId} not found.");
            }

            var newOrder = new Order
            {
                OrderDate = DateTime.UtcNow,
                TotalAmount = orderItemsList.Sum(item => item.Price * item.Quantity),
                CustomerId = dto.CustomerId,
                OrderItems = new List<OrderItem>()
            };

            for (int i = 0; i < orderItemsList.Count; i++)
            {
                var itemDto = orderItemsList[i];
                var newOrderItem = new OrderItem
                {
                    ItemName = itemDto.ItemName,
                    Quantity = itemDto.Quantity,
                    Service = itemDto.Service, 
                    Price = itemDto.Price,
                    CategoryId = itemDto.CategoryId
                };

                if (dto.ItemImages != null && i < dto.ItemImages.Count)
                {
                    var imageFile = dto.ItemImages[i];
                    if (imageFile?.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        await imageFile.CopyToAsync(memoryStream);
                        newOrderItem.ImageData = memoryStream.ToArray();
                    }
                }

                newOrder.OrderItems.Add(newOrderItem);
            }

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = newOrder.Id }, newOrder);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDto dto)
        {
            if (id != dto.Id) return BadRequest("Mismatched id.");
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

          
            if (!await _context.Customers.AnyAsync(c => c.Id == dto.CustomerId))
                return BadRequest("Customer not found.");

          
            if (dto.OrderItems == null || dto.OrderItems.Count == 0)
                return BadRequest("At least one item is required.");

            foreach (var item in dto.OrderItems)
            {
                if (string.IsNullOrWhiteSpace(item.ItemName))
                    return BadRequest("ItemName is required.");
                if (item.Quantity <= 0)
                    return BadRequest("Quantity must be greater than 0.");
                if (item.Price < 0)
                    return BadRequest("Price must be non-negative.");
                if (!await _context.Categories.AnyAsync(c => c.Id == item.CategoryId))
                    return BadRequest($"Category with ID {item.CategoryId} not found.");
            }

            
            order.CustomerId = dto.CustomerId;
        
            _context.OrderItems.RemoveRange(order.OrderItems);
            order.OrderItems = dto.OrderItems.Select(i => new OrderItem
            {
                ItemName = i.ItemName,
                Quantity = i.Quantity,
                Service = i.Service,
                Price = i.Price,
                CategoryId = i.CategoryId
            }).ToList();

            order.TotalAmount = order.OrderItems.Sum(x => x.Price * x.Quantity);

            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete("{id}")]
public async Task<IActionResult> DeleteOrder(int id)
{
    var order = await _context.Orders
        .Include(o => o.OrderItems)
        .FirstOrDefaultAsync(o => o.Id == id);

    if (order == null) return NotFound();

   
    if (order.OrderItems?.Any() == true)
        _context.OrderItems.RemoveRange(order.OrderItems);

    _context.Orders.Remove(order);
    await _context.SaveChangesAsync();
    return NoContent();
}


    }
}
