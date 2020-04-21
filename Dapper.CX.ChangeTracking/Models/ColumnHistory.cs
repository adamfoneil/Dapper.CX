using AO.DbSchema.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dapper.CX.ChangeTracking.Models
{
    [Identity(nameof(Id))]
    [UniqueConstraint(nameof(TableName), nameof(RowId), nameof(Version), nameof(ColumnName))]
    [Table("ColumnHistory", Schema = "changes")]
    public class ColumnHistory
    {
        public long Id { get; set; }

        [MaxLength(50)]
        [Required]
        public string UserName { get; set; }

        public DateTime Timestamp { get; set; }

        [Required]
        [MaxLength(100)]
        public string TableName { get; set; }

        public long RowId { get; set; }

        public int Version { get; set; }

        [MaxLength(100)]
        [Required]
        public string ColumnName { get; set; }        

        [Required]
        public string OldValue { get; set; }

        [Required]
        public string NewValue { get; set; }
    }
}
