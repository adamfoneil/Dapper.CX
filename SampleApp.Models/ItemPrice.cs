using AO.Models;
using Models.Conventions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SampleApp.Models
{
    /// <summary>
    /// defines the price of an Item at a certain PriceLevel
    /// </summary>
    public partial class ItemPrice : BaseTable
    {
        [References(typeof(Item))]
        [Key]
        public int ItemId { get; set; }

        [References(typeof(PriceLevel))]
        [Key]
        public int PriceLevelId { get; set; }

        [Column(TypeName = "money")]
        public decimal Price { get; set; }
    }
}
