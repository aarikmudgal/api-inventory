using System;
using System.Collections.Generic;
using System.Text;
using eshop.api.inventory.dal.Models;
using Microsoft.EntityFrameworkCore;

namespace eshop.api.inventory.dal.DBContext
{
    public class InventoryContext : DbContext
    {
        public InventoryContext(DbContextOptions<InventoryContext> options) : base(options)
        {

        }
        public DbSet<ArticleStock> ArticleStocks { get; set; }

        public bool CheckConnection()
        {
            try
            {
                this.Database.OpenConnection();
                this.Database.CloseConnection();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
