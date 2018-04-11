using System;
using System.Collections.Generic;
using System.Text;
using eshop.api.inventory.dal.DBContext;
using eshop.api.inventory.dal.Models;
using Microsoft.EntityFrameworkCore;

using System.Linq;

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

        public IEnumerable<ArticleStock> GetInventory()
        {
            try
            {
                return _context.ArticleStocks;
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        public ArticleStock GetArticleStock(int articleid)
        {
            return _context.ArticleStocks.SingleOrDefault(a => a.ArticleId == articleid);
        }

        public bool AddArticleStock(List<ArticleStock> articleStocks, out string statusMessage)
        {
            try
            {
                _context.ArticleStocks.AddRange(articleStocks);
                
                int status = _context.SaveChanges();
                
                statusMessage = "New articles add successfully to the inventory";
                return true;
            }
            catch (Exception)
            {
                //statusMessage = e.Message;
                //addedCustomer = null;
                throw;
            }
        }

        public bool UpdateArticleStock(int articleid, ArticleStock articleStock, out ArticleStock updatedArticleStock, out string statusMessage)
        {
            _context.Entry(articleStock).State = EntityState.Modified;

            try
            {
                if (!ArticleStockExists(articleid))
                {
                    updatedArticleStock = null;
                    statusMessage = $"Article ID {articleid} does not exist in the inventory";
                }


                _context.SaveChanges();
                statusMessage = $"Article stock updated successfully for article Id - {articleid}";
                updatedArticleStock = articleStock;
                return true;
            }
            catch (DbUpdateConcurrencyException e)
            {
                statusMessage = e.Message;
                throw e;
            }
        }
        private bool ArticleStockExists(int id)
        {
            return _context.ArticleStocks.Any(e => e.ArticleId == id);
        }

        public bool DeleteArticleStock(int articleid, out ArticleStock deletedArticleStock, out string statusMessage)
        {
            try
            {
                var articleStock = _context.ArticleStocks.SingleOrDefault(a => a.ArticleId == articleid);
                if (articleStock == null)
                {
                    deletedArticleStock = null;
                    statusMessage = $"Article with id - {articleid} not found in the inventory";
                    return false;
                }
                _context.ArticleStocks.Remove(articleStock);
                _context.SaveChanges();
                deletedArticleStock = articleStock;
                statusMessage = $"Article with id - {articleid} deleted successfully";
                return true;
            }
            catch (Exception e)
            {
                statusMessage = e.Message;
                deletedArticleStock = null;
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
