using Microsoft.EntityFrameworkCore;
using Prometheus;
using Service.Models;
using System.Diagnostics;
using System.Reflection.Emit;
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
    "service_uptime_seconds",
    "");


var activeRequests = Metrics.CreateGauge(
    "active_requests",
    "");

var RequestLatency =
    Metrics.CreateSummary(
        "api_request_duration_seconds",
        ""
     );

var avgRequestDuration = Metrics.CreateSummary(
    "http_request_avg_duration_ms",
    ""
    );
    


app.UseHttpMetrics();

app.Use(async (context, next) =>
{
    activeRequests.Inc();
  
    var sw = Stopwatch.StartNew();
    

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
        sw.Stop();
        var method = context.Request.Method;
        var path = context.Request.Path;
        RequestLatency.Observe(sw.Elapsed.TotalSeconds);
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

