using Microsoft.AspNetCore.Mvc;
using Prometheus;
using System.Collections.Concurrent;

namespace RetailApp.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class ProductController : ControllerBase
  {
    private readonly Counter _orderCounter;
    private readonly Gauge _inventoryGauge;
    private static ConcurrentDictionary<string, int> _inventory = new();

    public ProductController(Counter orderCounter, Gauge inventoryGauge)
    {
      _orderCounter = orderCounter;
      _inventoryGauge = inventoryGauge;

      // initialize stock if not set
      _inventory.TryAdd("laptop", 100);
      _inventoryGauge.WithLabels("laptop").Set(_inventory["laptop"]);
    }

    [HttpGet("products")]
    public IActionResult GetProducts()
    {
      return Ok(new[] { "laptop", "phone", "tshirt", "apples" });
    }

    [HttpPost("order")]
    public IActionResult PlaceOrder(string product, int qty = 1)
    {
      _orderCounter.Inc(qty);
      _inventory.AddOrUpdate(product, 100 - qty, (key, val) => val - qty);
      _inventoryGauge.WithLabels(product).Set(_inventory[product]);

      return Ok(new { product, qty, status = "PLACED" });
    }
  }
}
