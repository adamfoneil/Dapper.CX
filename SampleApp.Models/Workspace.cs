using Models.Conventions;
using System.ComponentModel.DataAnnotations;

namespace SampleApp.Models
{
    public partial class Workspace : BaseTable
    {
        [MaxLength(50)]
        [Key]
        public string Name { get; set; }
    }
}
