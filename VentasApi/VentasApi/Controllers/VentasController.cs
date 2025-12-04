using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VentasApi.Data;
using VentasApi.Models;
using System.Data;

[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    public VentasController(AppDbContext db, IConfiguration config) { _db = db; _config = config; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var ventas = await _db.Ventas
            .Include(v => v.Detalles!).ThenInclude(d => d.Producto)
            .Include(v => v.Cliente)
            .AsNoTracking()
            .ToListAsync();
        return Ok(ventas);
    }

    [HttpGet("cliente/{clienteId:int}")]
    public async Task<IActionResult> GetByCliente(int clienteId)
    {
        var ventas = await _db.Ventas
            .Where(v => v.ClienteId == clienteId)
            .Include(v => v.Detalles!).ThenInclude(d => d.Producto)
            .AsNoTracking()
            .ToListAsync();
        return Ok(ventas);
    }

    // DTO para recibir la venta desde el cliente
    public class VentaCreateDto
    {
        public int ClienteId { get; set; }
        public List<ItemDto> Items { get; set; } = new();
    }
    public class ItemDto { public int ProductoId { get; set; } public int Cantidad { get; set; } }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VentaCreateDto dto)
    {
        if (dto == null || dto.Items == null || !dto.Items.Any())
            return BadRequest("Datos de venta inválidos");

        // Llamada al stored procedure sp_RegistrarVenta
        using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();
        using var cmd = new SqlCommand("sp_RegistrarVenta", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        var dt = new DataTable();
        dt.Columns.Add("ProductoId", typeof(int));
        dt.Columns.Add("Cantidad", typeof(int));
        foreach (var item in dto.Items)
        {
            dt.Rows.Add(item.ProductoId, item.Cantidad);
        }

        var paramDetalles = new SqlParameter("@Detalles", SqlDbType.Structured)
        {
            TypeName = "dbo.DetalleVentaType",
            Value = dt
        };
        cmd.Parameters.Add(paramDetalles);
        cmd.Parameters.AddWithValue("@ClienteId", dto.ClienteId);

        var outputParam = new SqlParameter("@NewVentaId", SqlDbType.Int) { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(outputParam);

        try
        {
            await cmd.ExecuteNonQueryAsync();
            var newId = (int)outputParam.Value;
            var venta = await _db.Ventas
                .Include(v => v.Detalles!).ThenInclude(d => d.Producto)
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.Id == newId);

            return CreatedAtAction(nameof(GetByCliente), new { clienteId = dto.ClienteId }, venta);
        }
        catch (SqlException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
