using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YaungMel_POS.Database.Models;

public class Tbl_Summary
{
    [Key]
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public int TotalSale { get; set; }

    public decimal TotalAmount { get; set; }

    public int? TopSaleProductId { get; set; }

    public Tbl_Product? TopSaleProduct { get; set; }
}