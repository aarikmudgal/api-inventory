using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eshop.api.inventory.dal.DBContext;
using eshop.api.inventory.dal.Models;
using eshop.api.inventory.dal.Services;
using eshop.api.inventory.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace eshop.api.inventory.Controllers
{

    [Produces("application/json")]
    [Route("api/inventory")]
    public class InventoryController : Controller
    {
        private readonly InventoryContext _context;
        IInventoryService inventoryService;
        IConfiguration _configuration;

        public InventoryController(InventoryContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
            inventoryService = new InventoryService(context);
        }

        // GET api/customers/health
        [HttpGet]
        [Route("health")]
        public IActionResult GetHealth(string health)
        {
            bool dbConnOk = false;
            string statusMessage = string.Empty;
            try
            {
                _context.CheckConnection(out dbConnOk);
                statusMessage = $"Order service is Healthy";

            }
            catch (Exception ex)
            {
                statusMessage = $"Order database or service not available - {ex.Message}";

            }
            IActionResult response = dbConnOk ? Ok(statusMessage) : StatusCode(500, statusMessage);
            return response;
        }

        // GET: api/inventory
        [HttpGet]
        public IActionResult GetInventory()
        {
            try
            {
                return new ObjectResult(inventoryService.GetInventory());
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Error while getting inventory details - {ex.Message}");
            }

        }

        // GET: api/inventory/5
        [HttpGet("{articleid}")]
        public IActionResult GetArticleStock([FromRoute] int articleid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var articleStock = inventoryService.GetArticleStock(articleid);

            if (articleStock == null)
            {
                return NotFound($"Article with Id - {articleid} not found in the inventory");
            }

            return Ok(articleStock);
        }

        // PUT: api/inventory/5
        [HttpPut("{articleid}")]
        public IActionResult UpdateInventory([FromRoute] int articleid, [FromBody] ArticleStock articleStock)
        {
            string statusMessage;
            ArticleStock updatedArticleStock;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (articleid != articleStock.ArticleId)
            {
                return BadRequest();
            }
            try
            {
                bool status = inventoryService.UpdateArticleStock(articleid, articleStock, out updatedArticleStock, out statusMessage);
                if (updatedArticleStock == null)
                {
                    return NotFound($"Customer with id {articleid} not found");
                }
                JObject successobj = new JObject()
                {
                    { "StatusMessage", statusMessage },
                    { "ArticleStock", JObject.Parse(JsonConvert.SerializeObject(updatedArticleStock)) }
                };
                return Ok(successobj);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
                //throw;
            }

        }

        // POST: api/inventory
        [HttpPost]
        public IActionResult AddArticleStockToInventory([FromBody] List<ArticleStock> articleStocks)
        {
            
            string statusMessage;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                inventoryService.AddArticleStock(articleStocks,  out statusMessage);
                JObject successobj = new JObject()
                {
                    { "StatusMessage", statusMessage },
                    { "Customer", JArray.Parse(JsonConvert.SerializeObject(articleStocks)) }
                };
                return Ok(successobj);
            }
            catch (Exception ex)
            {

                return StatusCode(500, ex.Message + " Inner Exception- " + ex.InnerException.Message);
            }

        }

        // DELETE: api/inventory/5
        [HttpDelete("{articleid}")]
        public IActionResult DeleteArticleStock([FromRoute] int articleid)
        {
            ArticleStock deletedArticleStock;
            string statusMessage;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var status = inventoryService.DeleteArticleStock(articleid, out deletedArticleStock, out statusMessage);
                if (deletedArticleStock == null)
                {
                    return NotFound($"Article with id {articleid} not found in the inventory");
                }
                JObject successobj = new JObject()
                {
                    { "StatusMessage", statusMessage },
                    { "ArticleStock", JObject.Parse(JsonConvert.SerializeObject(deletedArticleStock)) }
                };
                return Ok(successobj);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        // GET: api/inventory/listen
        [HttpGet()]
        [Route("listen")]
        public IActionResult ListenForArticleStock()
        {

            string connectionString = _configuration.GetConnectionString("ConnectionString");

            Task.Factory.StartNew(() => {
                IInventoryConsumer consumer = new InventoryConsumer(connectionString);
                consumer.Listen();
            });


            //IInventoryConsumer consumer = new InventoryConsumer(inventoryService, _context);
            //consumer.Listen();

            return Ok("Consumer is continously listening for subscribed topics...");
        }

    }
}
