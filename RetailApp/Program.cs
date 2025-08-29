using Prometheus;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

//  Enable Prometheus metrics collection
app.UseMetricServer();   // exposes /metrics
app.UseHttpMetrics();    // collects default HTTP metrics (latency, count, etc.)

// Test route
app.MapGet("/", () => "Hello World!");

// Example custom metric (Gauge) with product label
var stockGauge = Metrics.CreateGauge(
    "myapp_inventory_stock",
    "Current stock level per product",
    new[] { "product" }
);

// Initialize stock for laptops so the metric shows up at startup
stockGauge.WithLabels("laptop").Set(10);

// Route to simulate buying a laptop
app.MapGet("/buy-laptop", () =>
{
    stockGauge.WithLabels("laptop").Dec(); // decrease stock
    return "Laptop purchased!";
});

// Route to simulate adding stock
app.MapGet("/restock-laptop/{count:int}", (int count) =>
{
    stockGauge.WithLabels("laptop").Inc(count); // increase stock
    return $"{count} laptops added!";
});

app.Run();
