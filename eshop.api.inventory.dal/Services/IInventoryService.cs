using eshop.api.inventory.dal.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eshop.api.inventory.dal.Services
{
    public interface IInventoryService
    {
        Task<IEnumerable<ArticleStock>> GetInventory();
        Task<ArticleStock> GetArticleStock(int articleid);
        Task<ReturnResult> AddArticleStock(List<ArticleStock> articleStocks);
        Task<ReturnResult> UpdateArticleStock(int articleId, ArticleStock articleStock);
        Task<ReturnResult> DeleteArticleStock(int articleid);
        bool ReduceArticleStock(int articleId, ArticleStock articleStock, out ArticleStock updatedArticleStock, out string statusMessage);
    }
}
