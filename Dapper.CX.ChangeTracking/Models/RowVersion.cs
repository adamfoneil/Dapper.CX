using AO.DbSchema.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dapper.CX.ChangeTracking.Models
{
    [Identity(nameof(Id))]
    [Table("RowVersion", Schema = "changes")]
    [UniqueConstraint(nameof(TableName), nameof(RowId))]
    public class RowVersion
    {
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string TableName { get; set; }

        public long RowId { get; set; }

        public int Version { get; set; }
    }
}
