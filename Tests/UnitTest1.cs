using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Models;
namespace Tests

{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public async Task Add100Elements_Simple_ShouldAddAllToDatabase()
        {
      
            var elementsToAdd = new List<Money_>();
            for (int i = 0; i < 100; i++)
            {
                elementsToAdd.Add(new Money_
                {
                    Name = $"Монета {i}",
                    Year_of_creation = 2000 + i,
                    Country = i % 2 == 0 ? "Россия" : "США"
                });
            }

          
            var initialResponse = await _client.GetAsync("api/money");
            var initialElements = await initialResponse.Content.ReadFromJsonAsync<List<Money_>>();
            int initialCount = initialElements?.Count ?? 0;

            
            foreach (var element in elementsToAdd)
            {
                var response = await _client.PostAsJsonAsync("api/money", element);
                response.EnsureSuccessStatusCode();
            }

        
            var finalResponse = await _client.GetAsync("api/money");
            var finalElements = await finalResponse.Content.ReadFromJsonAsync<List<Money_>>();

            Assert.AreEqual(initialCount + 100, finalElements.Count);
        }

    }
}
