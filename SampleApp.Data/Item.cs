using AO.Models;
using Models.Conventions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SampleApp.Data
{
    public class Item : BaseTable
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

        public bool IsActive { get; set; } = true;
    }
}
