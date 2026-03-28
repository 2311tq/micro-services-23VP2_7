using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TestProject2
{
    [TestClass]
    public class MoneyApiTests2
    {
        private HttpClient _httpClient2;

        [TestInitialize]

        public void Setup()
        {
            _httpClient2 = new HttpClient();
            _httpClient2.BaseAddress = new Uri("http://localhost:5048");


        }


        [TestCleanup]
        public void Cleanup()
        {
            _httpClient2?.Dispose();
        }

        [TestMethod]
        public async Task Add100Elements()
        {
            Random rnd = new Random();
            int successCount = 0;

            for (int i = 0; i < 100; i++)
            {
                var money = new
                {
                    Name = $" {i} ",
                    Year_of_creation = rnd.Next(1900, 2025),
                    Country = "Россия"
                };

                var json = JsonSerializer.Serialize(money);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient2.PostAsync("api/money", content);

                if (response.IsSuccessStatusCode)
                    successCount++;
            }

            Assert.AreEqual(100, successCount);
        }

        [TestMethod]
        public async Task Add10000Elements()
        {
            int successCount = 0;
            Random rnd = new Random();

            for (int i = 0; i < 10000; i++)
            {
                var money = new
                {
                    Name = $"{i}",
                    Year_of_creation = rnd.Next(1900, 2025),
                    Country = "Россия"
                };

                var json = JsonSerializer.Serialize(money);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient2.PostAsync("api/money", content);

                if (response.IsSuccessStatusCode)
                    successCount++;
            }

            Assert.AreEqual(10000, successCount);
        }

        [TestMethod]
        public async Task DeleteAllElements()
        {
            int DeletedCount = 0;


            while (true)
            {

                var getResponse = await _httpClient2.GetAsync("api/money");
                var content = await getResponse.Content.ReadAsStringAsync();

                if (content == "[]")
                    break;

                using JsonDocument doc = JsonDocument.Parse(content);
                var ids = new List<string>();

                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    if (element.TryGetProperty("id", out var id))
                        ids.Add(id.GetString());
                }

                if (ids.Count == 0)
                    break;


                foreach (var id in ids)
                {
                    var deleteResponse = await _httpClient2.DeleteAsync($"api/money/{id}");

                }
            }


            var finalResponse = await _httpClient2.GetAsync("api/money");
            var finalContent = await finalResponse.Content.ReadAsStringAsync();
            Assert.AreEqual("[]", finalContent);
        }
    }
}