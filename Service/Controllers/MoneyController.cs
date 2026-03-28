using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Service.Models;
using Prometheus;

namespace WebAPIApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoneyController : ControllerBase
    {
        private readonly IMongoCollection<Money> _moneyCollection;
      
        private static readonly Gauge MoneyCount =
    Metrics.CreateGauge(
        "money_collection_size",
        "");
        private static readonly Counter ErrorsTotal =
          Metrics.CreateCounter("api_errors_total", "");
        
        private static readonly Counter RequestsTotal =
  Metrics.CreateCounter("api_requests_total", "");

        private static readonly Histogram RequestLatency =
            Metrics.CreateHistogram(
                "api_request_duration_seconds",
                "",
                new HistogramConfiguration
                {
                    Buckets = Histogram.ExponentialBuckets(0.01, 2, 10)
                });


        public MoneyController()
        {
         
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("Numismatic_Club");
            _moneyCollection = database.GetCollection<Money>("Money");
            InitializeMetric();
        }
        private async void InitializeMetric()
        {
            var count = await _moneyCollection.CountDocumentsAsync(_ => true);
            MoneyCount.Set(count);
           
        }

        // GET api/money
        [HttpGet]
        public async Task<ActionResult<Money>> Get()
        {
            RequestsTotal.Inc();
            var totalCount = await _moneyCollection.CountDocumentsAsync(_ => true);

            List<Money> entities;

            if (totalCount > 1000)
            {
                
                entities = await _moneyCollection.Aggregate()
                    .Sample(1000)
                    .ToListAsync();
            }
            else
            {
               
                entities = await _moneyCollection.Find(_ => true).ToListAsync();
            }

            var result = entities.Select(m => new
            {
                m.Id,
                m.Name,
                m.Year_of_creation,
                m.Country
            });

            return Ok(result);
        }

        // GET api/money/{id}
        [HttpGet("{id}")]
       

      
        public async Task<ActionResult<Money_>> Get(string id)
        {
            RequestsTotal.Inc();

            using (RequestLatency.NewTimer())
            {
                var money = await _moneyCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

                if (money == null)
                    return NotFound();


                var result = new Money_
                {
                    Name = money.Name,
                    Year_of_creation = money.Year_of_creation,
                    Country = money.Country
                };

                return Ok(result);
            }
        }
        

        // POST api/money
        [HttpPost]
        public async Task<ActionResult<Money_>> Post([FromBody] Money_ moneyWithoutId)
        {
            RequestsTotal.Inc();

            using (RequestLatency.NewTimer())
            {
                try
                {
                  
                    if (moneyWithoutId == null)
                    {
                        ErrorsTotal.Inc();
                        return BadRequest();
                    }

                    if (string.IsNullOrWhiteSpace(moneyWithoutId.Name))
                    {
                        ErrorsTotal.Inc();
                        return BadRequest();
                    }

                    if (moneyWithoutId.Year_of_creation <= 0)
                    {
                        ErrorsTotal.Inc();
                        return BadRequest();
                    }

                    if (string.IsNullOrWhiteSpace(moneyWithoutId.Country))
                    {
                        ErrorsTotal.Inc();
                        return BadRequest();
                    }

                    var money = new Money
                    {
                        Name = moneyWithoutId.Name,
                        Year_of_creation = moneyWithoutId.Year_of_creation,
                        Country = moneyWithoutId.Country
                    };

                    await _moneyCollection.InsertOneAsync(money);

                  

                    var result = new Money_
                    {
                        Name = money.Name,
                        Year_of_creation = money.Year_of_creation,
                        Country = money.Country
                    };
                    MoneyCount.Inc();
                    return CreatedAtAction(nameof(Get), new { id = money.Id }, result);
                }
                catch
                {
                  
                    throw;
                }
            }
        }

        // PUT api/money/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(string id, [FromBody] Money_ moneyWithoutId)

        {

            RequestsTotal.Inc();
            using (RequestLatency.NewTimer())
            {
                if (moneyWithoutId == null) 
                {  
                  
                    return BadRequest(); 
                }
                 

                var existingMoney = await _moneyCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
                if (existingMoney == null)
                {
                    
                    return NotFound();
                }


                var updatedMoney = new Money
                {
                    Id = id,
                    Name = moneyWithoutId.Name,
                    Year_of_creation = moneyWithoutId.Year_of_creation,
                    Country = moneyWithoutId.Country
                };

                var result = await _moneyCollection.ReplaceOneAsync(
                    x => x.Id == id,
                    updatedMoney
                );

                if (result.MatchedCount == 0)
                {
                   
                    return NotFound();
                }


                var response = new Money_
                {
                    Name = updatedMoney.Name,
                    Year_of_creation = updatedMoney.Year_of_creation,
                    Country = updatedMoney.Country
                };

                return Ok(response);
            }
        }

        // DELETE api/money/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            RequestsTotal.Inc();
            var result = await _moneyCollection.DeleteOneAsync(x => x.Id == id);

            if (result.DeletedCount == 0)
            {
               
                return NotFound();
            }
            MoneyCount.Dec();
            return Ok();
        }

        // FILTER api/money/
        [HttpGet("filter")]
        public async Task<ActionResult<Money_>> Filter([FromQuery] int? year, [FromQuery] string comparison)
        {
            var query = _moneyCollection.AsQueryable();
            RequestsTotal.Inc();

            switch (comparison.ToLower())
                {
                    case "больше":
                        query = query.Where(x => x.Year_of_creation > year.Value);
                        break;

                    case "меньше":
                        query = query.Where(x => x.Year_of_creation < year.Value);
                        break;
                default:
                    {
                        ErrorsTotal.Inc();
                        return BadRequest("Значение сравнения разрешено использовать только 'больше' или 'меньше'");
                    }
                }
            
            var entities = await Task.Run(() => query.ToList());
            var result = entities.Select(m => new Money_
            {
                Name = m.Name,
                Year_of_creation = m.Year_of_creation,
                Country = m.Country
            });

            return Ok(result);
        }
    }
}