using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

public class VentasController : Controller
{
    private readonly IHttpClientFactory _http;
    public VentasController(IHttpClientFactory http) { _http = http; }

    public async Task<IActionResult> Crear()
    {
        var client = _http.CreateClient("VentasApi");
        var clientes = await client.GetFromJsonAsync<List<ClienteVm>>("api/clientes");
        var productos = await client.GetFromJsonAsync<List<ProductoVm>>("api/productos");
        var model = new CrearVentaVm { Clientes = clientes ?? new(), Productos = productos ?? new() };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromForm] CrearVentaSubmit model)
    {
        var client = _http.CreateClient("VentasApi");
        var payload = new
        {
            clienteId = model.ClienteId,
            items = model.Items.Select(i => new { productoId = i.ProductoId, cantidad = i.Cantidad }).ToList()
        };
        var resp = await client.PostAsJsonAsync("api/ventas", payload);
        if (resp.IsSuccessStatusCode)
        {
            TempData["Success"] = "Venta registrada correctamente.";
            return RedirectToAction("Index", "Productos");
        }
        var text = await resp.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, text);

        var clientes = await client.GetFromJsonAsync<List<ClienteVm>>("api/clientes");
        var productos = await client.GetFromJsonAsync<List<ProductoVm>>("api/productos");
        var vm = new CrearVentaVm { Clientes = clientes ?? new(), Productos = productos ?? new(), SelectedItems = model.Items };
        return View(vm);
    }
}

public record ClienteVm(int Id, string Nombre, string Email);
public record ProductoVm(int Id, string Nombre, decimal Precio, int Stock);
public class CrearVentaVm
{
    public List<ClienteVm> Clientes { get; set; } = new();
    public List<ProductoVm> Productos { get; set; } = new();
    public int ClienteId { get; set; }
    public List<ItemForm> SelectedItems { get; set; } = new();
}
public class ItemForm { public int ProductoId { get; set; } public int Cantidad { get; set; } }
public class CrearVentaSubmit { public int ClienteId { get; set; } public List<ItemForm> Items { get; set; } = new(); }
