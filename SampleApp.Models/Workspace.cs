using Models.Conventions;
using System.ComponentModel.DataAnnotations;

namespace SampleApp.Models
{
    public class Workspace : BaseTable
    {
        [MaxLength(50)]
        [Key]
        public string Name { get; set; }
    }
}
