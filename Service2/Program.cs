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
builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = "localhost";
    options.InstanceName = "service2";
});

var app = builder.Build();
var uptimeTimer = Stopwatch.StartNew();

var uptimeMetric = Metrics.CreateGauge(
    "service_uptime_seconds2",
    "");



<<<<<<< HEAD

=======
>>>>>>> 027f1764bf6c9d9b2db273a62993dfe0e6b6c170
var RequestLatency =
    Metrics.CreateSummary(
        "api_request_duration_seconds",
        ""
     );
<<<<<<< HEAD
=======
var avgRequestDuration = Metrics.CreateSummary(
    "http_request_avg_duration_ms",
    ""
    );
>>>>>>> 027f1764bf6c9d9b2db273a62993dfe0e6b6c170





app.UseHttpMetrics();

app.Use(async (context, next) =>
{
<<<<<<< HEAD
   
=======
    activeRequests.Inc();
>>>>>>> 027f1764bf6c9d9b2db273a62993dfe0e6b6c170
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
<<<<<<< HEAD
      
=======
        activeRequests.Dec();
>>>>>>> 027f1764bf6c9d9b2db273a62993dfe0e6b6c170
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


