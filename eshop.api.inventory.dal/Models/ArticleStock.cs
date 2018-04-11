using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace eshop.api.inventory.dal.Models
{
    public class ArticleStock
    {
        [Key]
        public int ArticleId { get; set; }
        public string ArticleName { get; set; }
        public int TotalQuantity { get; set; }
    }
}
