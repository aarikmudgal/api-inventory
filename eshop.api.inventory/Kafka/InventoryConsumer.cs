using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using eshop.api.inventory.dal.DBContext;
using eshop.api.inventory.dal.Models;
using eshop.api.inventory.dal.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace eshop.api.inventory.Kafka
{
    public class InventoryConsumer : IInventoryConsumer
    {
        private const string articlesTopic = "articles-topic";
        private string connectionString;


        public InventoryConsumer(string conString)
        {
            connectionString = conString;
        }
        public void Listen()
        {
            
            var config = new Dictionary<string, object>
            {
                {"group.id", "simple_consumer"},
                {"bootstrap.servers", "35.200.201.3:9092" },
                {"enable.auto.commit", "false" }
            };


            using (var consumer = new Consumer<Null, string>(config, null, new StringDeserializer(Encoding.UTF8)))
            {
                string statusMessage;
                ArticleStock updatedStock;

                consumer.Subscribe(articlesTopic);
                Console.WriteLine("Consumer started...");

                consumer.OnMessage += (_, msg) =>
                {
                    DbContextOptions<InventoryContext> options = new DbContextOptionsBuilder<InventoryContext>()
                            .UseNpgsql(connectionString)
                            .Options;
                    InventoryContext context = new InventoryContext(options);

                    IInventoryService service = new InventoryService(context);

                    Console.WriteLine("JSON Array string ");
                    //messgae(msg.Value);
                    List<ArticleStock> articles = JsonConvert.DeserializeObject<List<ArticleStock>>(msg.Value);

                    Console.WriteLine("Iterating the list");
                    foreach (var item in articles)
                    {
                        Console.WriteLine($"Article id: {item.ArticleId}, Article Name: {item.ArticleName}, Quantity: {item.TotalQuantity}");
                        service.ReduceArticleStock(item.ArticleId, item, out updatedStock, out statusMessage);
                    }

                };

                while (true)
                {
                    consumer.Poll(100);
                }
            }
        }
    }
}
