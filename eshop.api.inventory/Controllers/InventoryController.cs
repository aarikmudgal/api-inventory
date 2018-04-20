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
        public async Task<IActionResult> GetInventory()
        {
            try
            {
                var result = await inventoryService.GetInventory();
                return new ObjectResult(result);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Error while getting inventory details - {ex.Message}");
            }

        }

        // GET: api/inventory/5
        [HttpGet("{articleid}")]
        public async Task<IActionResult> GetArticleStock([FromRoute] int articleid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var articleStock = await inventoryService.GetArticleStock(articleid);

            if (articleStock == null)
            {
                return NotFound($"Article with Id - {articleid} not found in the inventory");
            }

            return Ok(articleStock);
        }

        // PUT: api/inventory/5
        [HttpPut("{articleid}")]
        public async Task<IActionResult> UpdateInventory([FromRoute] int articleid, [FromBody] ArticleStock articleStock)
        {
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
                ReturnResult result = await inventoryService.UpdateArticleStock(articleid, articleStock);
                if (result.UpdatedArticleStock == null)
                {
                    return NotFound($"Customer with id {articleid} not found");
                }
                JObject successobj = new JObject()
                {
                    { "StatusMessage", result.StatusMessage },
                    { "ArticleStock", JObject.Parse(JsonConvert.SerializeObject(result.UpdatedArticleStock)) }
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
        public async Task<IActionResult> AddArticleStockToInventory([FromBody] List<ArticleStock> articleStocks)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                ReturnResult result = await inventoryService.AddArticleStock(articleStocks);
                JObject successobj = new JObject()
                {
                    { "StatusMessage", result.StatusMessage },
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
        public async Task<IActionResult> DeleteArticleStock([FromRoute] int articleid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //ReturnResult result = new ReturnResult();
            try
            {
                ReturnResult result = await inventoryService.DeleteArticleStock(articleid);
                if (result.UpdatedArticleStock == null)
                {
                    return NotFound($"Article with id {articleid} not found in the inventory");
                }
                JObject successobj = new JObject()
                {
                    { "StatusMessage", result.StatusMessage },
                    { "ArticleStock", JObject.Parse(JsonConvert.SerializeObject(result.UpdatedArticleStock)) }
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
