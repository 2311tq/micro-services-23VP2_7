using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using Prometheus;
using Service.Models;
using StackExchange.Redis;
using System.Text.Json;


namespace WebAPIApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoneyController : ControllerBase
    {
        private readonly IMongoCollection<Money> _moneyCollection;
        private readonly IDistributedCache _cache;
       
        private static readonly Counter RequestsTotal =
    Metrics.CreateCounter("api_requests_total2", "");
        private static readonly Gauge MoneyCount =
    Metrics.CreateGauge(
        "money_collection_size",
        "");

        private static readonly Counter ErrorsTotal2 =
            Metrics.CreateCounter("api_errors_total2", "");
        public MoneyController(IDistributedCache cache)
        {
            _cache = cache;
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("Numismatic_Club2");
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
            string cacheKey = "money_all";

            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                var data = JsonSerializer.Deserialize<object>(cached);
                return Ok(data);
            }
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
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), options);

            return Ok(result);
        }

        // GET api/money/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Money_>> Get(string id)
        {
            RequestsTotal.Inc();


            {
                if (id.Length != 24)
                {
                    ErrorsTotal2.Inc();
                    return NotFound("id состоит из 24 символов");
                }
                string cacheKey = $"money_{id}";

                var cached = await _cache.GetStringAsync(cacheKey);
                if (cached != null)
                {
                    var data = JsonSerializer.Deserialize<Money_>(cached);
                    return Ok(data);
                }
                var money = await _moneyCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

                if (money == null)
                {
                    ErrorsTotal2.Inc();
                    return NotFound();
                }
                var result = new Money_
                {
                    Name = money.Name,
                    Year_of_creation = money.Year_of_creation,
                    Country = money.Country
                };
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), options);
                return Ok(result);
            }
        }


        // POST api/money
        [HttpPost]
        public async Task<ActionResult<Money_>> Post([FromBody] Money_ moneyWithoutId)
        {
            RequestsTotal.Inc();


            {
                try
                {

                    if (moneyWithoutId == null)
                    {
                        ErrorsTotal2.Inc();
                        return BadRequest();
                    }

                    if (string.IsNullOrWhiteSpace(moneyWithoutId.Name))
                    {
                        ErrorsTotal2.Inc();
                        return BadRequest();
                    }

                    if (moneyWithoutId.Year_of_creation <= 0)
                    {
                        ErrorsTotal2.Inc();
                        return BadRequest();
                    }

                    if (string.IsNullOrWhiteSpace(moneyWithoutId.Country))
                    {
                        ErrorsTotal2.Inc();
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
                    await _cache.RemoveAsync("money_all");
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
            if (id.Length != 24)
            {
                ErrorsTotal2.Inc();
                return NotFound("id состоит из 24 символов");
            }

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
                await _cache.RemoveAsync("money_all");
                await _cache.RemoveAsync($"money_{id}");

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

            if (id.Length != 24)
            {
                ErrorsTotal2.Inc();
                return NotFound("id состоит из 24 символов");
            }

            var result = await _moneyCollection.DeleteOneAsync(x => x.Id == id);

            if (result.DeletedCount == 0)
            {
                ErrorsTotal2.Inc();
                return NotFound();
            }



            await _cache.RemoveAsync($"money_{id}");
            await _cache.RemoveAsync("money_all");
            MoneyCount.Dec();
            return Ok();
        }

        // FILTER api/money/
        [HttpGet("filter")]
        public async Task<ActionResult<Money_>> Filter([FromQuery] int? year, [FromQuery] string comparison)
        {
            var query = _moneyCollection.AsQueryable();
            RequestsTotal.Inc();
            string cacheKey = $"money_filter_{comparison}_{year}";

            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                var data = JsonSerializer.Deserialize<Money_>(cached);
                return Ok(data);
            }
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
                        ErrorsTotal2.Inc();
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
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), options);
            return Ok(result);
        }
    }
}
