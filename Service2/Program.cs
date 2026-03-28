using Microsoft.EntityFrameworkCore;
using Prometheus;
using Service.Models;
using System.Diagnostics;
using WebAPIApp.Models;
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
var uptimeTimer = Stopwatch.StartNew();

var uptimeMetric = Metrics.CreateGauge(
    "service_uptime_seconds2",
    "");


var activeRequests = Metrics.CreateGauge(
    "active_requests2",
    "");

var cpuUsageMetric = Metrics.CreateGauge(
    "cpu_usage_rate2",
    "",
    new GaugeConfiguration
    {
        LabelNames = new[] { "core" }
    });





app.UseHttpMetrics();

app.Use(async (context, next) =>
{
    activeRequests.Inc();
    

    uptimeMetric.Set(uptimeTimer.Elapsed.TotalSeconds);


    try
    {
        await next();
    }
    catch
    {
       
        throw;
    }
    finally
    {
        activeRequests.Dec();
    }
});

app.MapMetrics();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseAuthorization();

app.MapControllers();

app.Run();

