using eshop.api.inventory.dal.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eshop.api.inventory.dal.Services
{
    public interface IInventoryService
    {
        IEnumerable<ArticleStock> GetInventory();
        ArticleStock GetArticleStock(int articleid);
        bool AddArticleStock(List<ArticleStock> articleStocks, out string statusMessage);
        bool UpdateArticleStock(int articleId, ArticleStock articleStock, out ArticleStock updatedArticleStock, out string statusMessage);
        bool DeleteArticleStock(int articleid, out ArticleStock articleStock, out string statusMessage);
        bool ReduceArticleStock(int articleId, ArticleStock articleStock, out ArticleStock updatedArticleStock, out string statusMessage);
    }
}
