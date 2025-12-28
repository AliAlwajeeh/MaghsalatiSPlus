
using MaghsalatiSPlus.Data;
using MaghsalatiSPlus.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaghsalatiSPlus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await _context.Customers.AsNoTracking().ToListAsync();
        }

        
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }
            return Ok(customer); 
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, [FromBody] CreateCustomerDtoWithId customerDto)
        {
            var customerToUpdate = await _context.Customers.FindAsync(id);
            if (customerToUpdate == null)
            {
                return NotFound();
            }

            if (customerToUpdate.ShopOwnerId != customerDto.ShopOwnerId)
            {
                return BadRequest("Cannot change the owner of a customer.");
            }

            customerToUpdate.Name = customerDto.Name;
            customerToUpdate.PhoneNumber = customerDto.PhoneNumber;

            _context.Entry(customerToUpdate).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

      
        [HttpPost]
        public async Task<ActionResult<Customer>> PostCustomer([FromBody] CreateCustomerDtoWithId customerDto)
        {
          
            var ownerExists = await _context.Users.AnyAsync(u => u.Id == customerDto.ShopOwnerId);
            if (!ownerExists)
            {
                return BadRequest(new { Message = $"ShopOwner with ID '{customerDto.ShopOwnerId}' not found." });
            }

            var newCustomer = new Customer
            {
                Name = customerDto.Name,
                PhoneNumber = customerDto.PhoneNumber,
                ShopOwnerId = customerDto.ShopOwnerId
            };

            await _context.Customers.AddAsync(newCustomer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomer), new { id = newCustomer.Id }, newCustomer);
        }

  
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}