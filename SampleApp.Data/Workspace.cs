using Models.Conventions;
using System.ComponentModel.DataAnnotations;

namespace SampleApp.Data
{
    public class Workspace : BaseTable
    {
        [MaxLength(50)]
        [Key]
        public string Name { get; set; }
    }
}
