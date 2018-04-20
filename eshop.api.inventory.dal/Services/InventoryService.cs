using System;
using System.Collections.Generic;
using System.Text;
using eshop.api.inventory.dal.DBContext;
using eshop.api.inventory.dal.Models;
using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Threading.Tasks;

namespace eshop.api.inventory.dal.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly InventoryContext _context;
        public InventoryService(InventoryContext context)
        {
            _context = context;

            CheckConnection();
        }

        private void CheckConnection()
        {
            try
            {
                _context.Database.GetDbConnection();
                _context.Database.OpenConnection();
            }
            catch (Exception)
            {
                // log db connectivity issue
                throw;
            }
        }

        public async Task<IEnumerable<ArticleStock>> GetInventory()
        {
            try
            {
                return await _context.ArticleStocks.ToListAsync(); ;
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        public async Task<ArticleStock> GetArticleStock(int articleid)
        {
            return await _context.ArticleStocks.SingleOrDefaultAsync(a => a.ArticleId == articleid);
        }

        public async Task<ReturnResult> AddArticleStock(List<ArticleStock> articleStocks)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                await _context.ArticleStocks.AddRangeAsync(articleStocks);
                
                await _context.SaveChangesAsync();

                result.StatusMessage = "New articles add successfully to the inventory";
                return result;
            }
            catch (Exception)
            {
                //statusMessage = e.Message;
                //addedCustomer = null;
                throw;
            }
        }

        public async Task<ReturnResult> UpdateArticleStock(int articleid, ArticleStock articleStock)
        {
            _context.Entry(articleStock).State = EntityState.Modified;
            ReturnResult result = new ReturnResult();
            try
            {
                if (!ArticleStockExists(articleid))
                {
                    result.UpdatedArticleStock = null;
                    result.StatusMessage = $"Article ID {articleid} does not exist in the inventory";
                }


                await _context.SaveChangesAsync();
                result.StatusMessage = $"Article stock updated successfully for article Id - {articleid}";
                result.UpdatedArticleStock = articleStock;
                return result;
            }
            catch (DbUpdateConcurrencyException e)
            {
                result.StatusMessage = e.Message;
                throw e;
            }
        }
        private bool ArticleStockExists(int id)
        {
            return _context.ArticleStocks.Any(e => e.ArticleId == id);
        }

        public async Task<ReturnResult> DeleteArticleStock(int articleid)
        {
            ReturnResult result = new ReturnResult();
            try
            {
                var articleStock = await _context.ArticleStocks.SingleOrDefaultAsync(a => a.ArticleId == articleid);
                if (articleStock == null)
                {
                    result.UpdatedArticleStock = null;
                    result.StatusMessage = $"Article with id - {articleid} not found in the inventory";
                    return result;
                }
                _context.ArticleStocks.Remove(articleStock);
                await _context.SaveChangesAsync();
                result.UpdatedArticleStock = articleStock;
                result.StatusMessage = $"Article with id - {articleid} deleted successfully";
                return result;
            }
            catch (Exception e)
            {
                result.StatusMessage = e.Message;
                result.UpdatedArticleStock = null;
                throw e;
            }
        }

        public bool ReduceArticleStock(int articleId, ArticleStock articleStock, out ArticleStock updatedArticleStock, out string statusMessage)
        {
            //_context.Entry(articleStock).State = EntityState.Modified;

            try
            {
                if (!ArticleStockExists(articleId))
                {
                    updatedArticleStock = null;
                    statusMessage = $"Article ID {articleId} does not exist in the inventory";
                }

                ArticleStock articleStockToUpdate = _context.ArticleStocks.Where(a => a.ArticleId == articleId).FirstOrDefault();
                articleStockToUpdate.TotalQuantity = articleStockToUpdate.TotalQuantity - articleStock.TotalQuantity;

                _context.SaveChanges();
                statusMessage = $"Article stock updated successfully for article Id - {articleId}";
                updatedArticleStock = articleStock;
                return true;
            }
            catch (DbUpdateConcurrencyException e)
            {
                statusMessage = e.Message;
                throw e;
            }
        }
    }
}
