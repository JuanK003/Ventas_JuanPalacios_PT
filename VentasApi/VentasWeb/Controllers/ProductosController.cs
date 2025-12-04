using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

public class ProductosController : Controller
{
    private readonly IHttpClientFactory _http;
    public ProductosController(IHttpClientFactory http) { _http = http; }

    public async Task<IActionResult> Index()
    {
        var client = _http.CreateClient("VentasApi");
        var productos = await client.GetFromJsonAsync<List<ProductoViewModel>>("api/productos");
        return View(productos);
    }
}

public class ProductoViewModel { public int Id { get; set; } public string Nombre { get; set; } = null!; public decimal Precio { get; set; } public int Stock { get; set; } }
