using AO.Models;
using System.ComponentModel.DataAnnotations;

namespace Dapper.CX.Models
{
    [Identity(nameof(Id))]
    [Schema("changes")]
    public class RowVersion
    {
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Key]
        public string TableName { get; set; }

        [Key]
        public long RowId { get; set; }

        public int Version { get; set; }
    }
}
