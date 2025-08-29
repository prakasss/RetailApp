Monitoring .NET Core API with Prometheus & Grafana

---

Task 1: Install Prometheus

## Download Prometheus for Windows:

curl -LO https://github.com/prometheus/prometheus/releases/download/v2.30.0/prometheus-2.30.0.windows-amd64.zip
tar -xvf prometheus-2.30.0.windows-amd64.zip
cd prometheus-2.30.0.windows-amd64

2. Edit prometheus.yml to scrape the .NET Core API:

---

global:
scrape_interval: 5s

scrape_configs:

- job_name: "dotnet-app"
  metrics_path: "/metrics"
  static_configs:
  - targets: ["host.docker.internal:5293"] # replace 5293 with your API port

3. Start Prometheus:

---

prometheus.exe --config.file=prometheus.yml

4. Access Prometheus UI at:
   http://localhost:9090

## Task 2: Configure .NET Core Application for Metrics

Install Prometheus.AspNetCore NuGet package:

## dotnet add package prometheus-net.AspNetCore

2. Program.cs:

using Prometheus;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Enable Prometheus metrics collection
app.UseMetricServer(); // exposes /metrics
app.UseHttpMetrics(); // collects default HTTP metrics (latency, count, etc.)

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

3. Run the .NET Core API and test metrics endpoint:
   http://localhost:5293/metrics

## Task 3: Install and Configure Grafana

Download and install Grafana for Windows:
https://grafana.com/grafana/download

## Start Grafana service:

- net start grafana

or
Stop and remove the old container
docker stop grafana
docker rm grafana

---

Now you can re-run:

docker run -d -p 3000:3000 --name=grafana grafana/grafana

List all containers to confirm
docker ps -a

---

Access Grafana:
👉 http://localhost:3000 (default login: admin/admin)

## Add Prometheus as a Data Source:

Go to Configuration > Data Sources > Add data source

Select Prometheus

Set URL: http://localhost:9090

Click Save & Test

## Task 4: Create a Metrics Dashboard

Create a new dashboard in Grafana.

Add Time Series Panel to track API requests:

## PromQL:

http_requests_received_total

Add Gauge Panel for custom stock metric:

PromQL:

myapp_inventory_stock{product="laptop"}

Save the dashboard.

## Validation of the Solution

Check Prometheus Targets
👉 http://localhost:9090/targets
Ensure dotnet_app is UP.

Verify Metrics in Prometheus
Run PromQL queries like:

up
myapp_inventory_stock

Check Grafana Dashboard

Ensure graphs update when API calls are made.

Adjust time range and filters interactively.

Simulate Load
Run:

curl http://localhost:5293/buy-laptop

## Watch myapp_inventory_stock{product="laptop"} decrease in Grafana.

Conclusion

This project successfully implements Prometheus + Grafana monitoring for .NET Core API, including:

Built-in HTTP metrics

Custom Gauge metric for stock inventory

Real-time dashboards with Grafana

This setup allows proactive monitoring, quick issue detection, and better system reliability.
