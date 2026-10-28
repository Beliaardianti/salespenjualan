using BelajarAspNetMvc.Data;
using BelajarAspNetMvc.Dtos;
using BelajarAspNetMvc.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BelajarAspNetMvc.Controllers;

// Pelanggan boleh dikelola Admin maupun Kasir (kasir sering mendaftarkan pelanggan baru)
[ApiController]
[Route("customers")]
[Authorize]
public class CustomersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<CustomerDto>> GetAll(string? search)
    {
        var query = db.Customers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(c => EF.Functions.ILike(c.Name, pattern) || EF.Functions.ILike(c.Phone ?? "", pattern));
        }

        return await query.OrderBy(c => c.Name)
            .Select(c => new CustomerDto(c.Id, c.Name, c.Phone, c.Email, c.Address))
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CustomerRequest req)
    {
        var customer = new Customer();
        Apply(customer, req);
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return new CustomerDto(customer.Id, customer.Name, customer.Phone, customer.Email, customer.Address);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CustomerRequest req)
    {
        var customer = await db.Customers.FindAsync(id);
        if (customer is null) return NotFound();

        Apply(customer, req);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // Hapus pelanggan: transaksi lamanya tetap ada, kolom CustomerId jadi NULL ("Umum")
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await db.Customers.FindAsync(id);
        if (customer is null) return NotFound();

        db.Customers.Remove(customer);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // string kosong "" dari form dianggap null
    private static void Apply(Customer c, CustomerRequest req)
    {
        c.Name = req.Name.Trim();
        c.Phone = string.IsNullOrWhiteSpace(req.Phone) ? null : req.Phone.Trim();
        c.Email = string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.Trim();
        c.Address = string.IsNullOrWhiteSpace(req.Address) ? null : req.Address.Trim();
    }
}
