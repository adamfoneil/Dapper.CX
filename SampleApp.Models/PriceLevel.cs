using AO.Models;
using Models.Conventions;
using System.ComponentModel.DataAnnotations;

namespace SampleApp.Models
{
    public class PriceLevel : BaseTable
    {
        [References(typeof(Workspace))]
        [Key]
        public int WorkspaceId { get; set; }

        [Key]
        [MaxLength(50)]
        public string Name { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
