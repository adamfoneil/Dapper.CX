using AO.Models;
using Models.Conventions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SampleApp.Models
{
    public partial class Item : BaseTable
    {
        [References(typeof(Workspace))]
        [Key]
        public int WorkspaceId { get; set; }

        [Key]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [Column(TypeName = "money")]
        public decimal UnitCost { get; set; }

        [Column(TypeName = "money")]
        public decimal SalePrice { get; set; }

        // demo property for ModelSync
        //public int OnHandQuantity { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
