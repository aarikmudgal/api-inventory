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

        public void CheckConnection(out bool dbStatusOK)
        {
            try
            {

                this.Database.OpenConnection();
                this.Database.ExecuteSqlCommand("SELECT 1");
                this.Database.CloseConnection();
                dbStatusOK = true;
            }
            catch (Exception ex)
            {
                dbStatusOK = false;
                throw ex;
            }
            finally
            {
                this.Database.CloseConnection();
            }
        }
    }
}
