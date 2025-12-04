using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using VentasApi.Data;
using VentasApi.Models;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ClientesController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Clientes.AsNoTracking().ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Cliente cliente)
    {
        if (cliente == null) return BadRequest();

        if (!IsValidEmail(cliente.Email)) return BadRequest("Email inválido");

        // Verificar unicidad
        if (await _db.Clientes.AnyAsync(c => c.Email == cliente.Email))
            return Conflict("Email ya registrado");

        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = cliente.Id }, cliente);
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        var re = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        return re.IsMatch(email);
    }
}
