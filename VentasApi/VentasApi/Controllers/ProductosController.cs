using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VentasApi.Data;
using VentasApi.Models;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProductosController(AppDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var prods = await _db.Productos.AsNoTracking().ToListAsync();
        return Ok(prods);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var p = await _db.Productos.FindAsync(id);
        if (p == null) return NotFound();
        return Ok(p);
    }
}
