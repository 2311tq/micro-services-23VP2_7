using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace TestProject3
{
    [TestClass]
    public class MoneyApiTests2

    {
        private HttpClient _httpClient;

        [TestInitialize]


        public void Setup()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5177");


        }


        [TestMethod]
        public async Task Add100Elements1()

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

                var response = await _httpClient.PostAsync("api/money", content);

                if (response.IsSuccessStatusCode)
                    successCount++;
            }

            Assert.AreEqual(100, successCount);
        }

        [TestMethod]

        public async Task Add10000Elements1()

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

                var response = await _httpClient.PostAsync("api/money", content);

                if (response.IsSuccessStatusCode)
                    successCount++;
            }

            Assert.AreEqual(10000, successCount);
        }

        [TestMethod]

        public async Task DeleteAllElements1()
        {
            while (true)
            {
                var getResponse = await _httpClient.GetAsync("api/money");
                var content = await getResponse.Content.ReadAsStringAsync();

                using JsonDocument doc = JsonDocument.Parse(content);

                if (doc.RootElement.GetArrayLength() == 0)
                    break;


                var ids = new List<string>();

                foreach (var element in doc.RootElement.EnumerateArray())
                {

                    if (element.TryGetProperty("Id", out var id))
                        ids.Add(id.GetString());
                }

                foreach (var id in ids)
                {
                    await _httpClient.DeleteAsync($"api/money/{id}");
                }

                await Task.Delay(100);
            }


            await Task.Delay(200);

            var finalResponse = await _httpClient.GetAsync("api/money");
            var finalContent = await finalResponse.Content.ReadAsStringAsync();

            using JsonDocument finalDoc = JsonDocument.Parse(finalContent);

            Assert.AreEqual(0, finalDoc.RootElement.GetArrayLength());

        }
    }
}